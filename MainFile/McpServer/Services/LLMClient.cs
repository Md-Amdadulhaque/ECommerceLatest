using System.Net;
using System.Text.Json;
using MCP_Server.Models;

namespace MCP_Server.Services
{
    public class LLMClient
    {
        private const int MaxAttempts = 3;

        private readonly HttpClient _httpClient;
        private readonly ILogger<LLMClient> _logger;
        private readonly string _baseUrl;
        private readonly string _model;

        public LLMClient(HttpClient http, IConfiguration config, ILogger<LLMClient> logger)
        {
            _httpClient = http;
            _logger = logger;
            var apiKey = config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Gemini API key not configured (dotnet user-secrets set Gemini:ApiKey <key>)");
            _baseUrl = config["Gemini:BaseUrl"] ?? throw new InvalidOperationException("Gemini BaseUrl not configured");
            _model = config["Gemini:Model"] ?? throw new InvalidOperationException("Gemini Model not configured");

            // Key goes in a header, not the URL, so it never ends up in logs or traces.
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
        }

        public static object UserText(string text) =>
            new { role = "user", parts = new[] { new { text } } };

        public static object ToolResults(IEnumerable<(string Name, object? Result)> results) =>
            new
            {
                role = "user",
                parts = results.Select(r => new
                {
                    functionResponse = new { name = r.Name, response = new { result = r.Result } }
                }).ToArray()
            };

        // One model turn: sends the whole conversation so far and returns either
        // a text reply or the tool calls the model wants to make.
        public async Task<LLMResponse> GenerateAsync(IReadOnlyList<object> contents, List<ToolDefinition> tools)
        {
            var request = new
            {
                systemInstruction = new { parts = new[] { new { text = SystemMessage() } } },
                contents,
                tools = new[] { new { functionDeclarations = tools } }
            };

            var url = $"{_baseUrl}/models/{_model}:generateContent";

            for (int attempt = 1; ; attempt++)
            {
                using var response = await _httpClient.PostAsJsonAsync(url, request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    return ParseResponse(result);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                bool retryable = response.StatusCode is HttpStatusCode.TooManyRequests
                    or HttpStatusCode.ServiceUnavailable or HttpStatusCode.InternalServerError;

                if (!retryable || attempt == MaxAttempts)
                {
                    _logger.LogError("Gemini returned {Status}: {Error}", (int)response.StatusCode, errorContent);
                    throw new HttpRequestException($"Gemini API returned {(int)response.StatusCode}", null, response.StatusCode);
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning("Gemini returned {Status}, retrying in {Delay}s (attempt {Attempt}/{Max})",
                    (int)response.StatusCode, delay.TotalSeconds, attempt, MaxAttempts);
                await Task.Delay(delay);
            }
        }

        private string SystemMessage()
        {
            var systemPrompt = @"You are a helpful AI shopping assistant with access to a product database through specialized tools.

CRITICAL TOOL USAGE INSTRUCTIONS:

1. ALWAYS READ TOOL DESCRIPTIONS CAREFULLY
   - Each tool has a specific purpose described in its [Description] attribute
   - Parameter descriptions tell you EXACTLY what values to provide
   - You MUST use the appropriate tool for each type of user request

2. MANDATORY TOOL USAGE RULES:

   When user asks about PRODUCTS BY CATEGORY:
   ✓ FIRST call CategoriesList() to get valid category names
   ✓ THEN call GetProductsByCategory() with the EXACT spelling from the list
   ✗ NEVER guess category spellings

   When user asks for CHEAP/BUDGET products:
   ✓ Call GetCheapestProducts(count)
   ✓ If user doesn't specify count, use default value 5

   When user asks to FILTER products (price range, color, brand, specific criteria):
   ✓ FIRST call GetProductProperties() to understand available filter properties
   ✓ THEN call FilterProducts() with filters in 'property:value' format
   ✓ Examples: 'Category:Laptop', 'MinPrice:500', 'MaxPrice:1500', 'Color:Silver'
   ✓ You can pass MULTIPLE filters in a single call


   When user wants to CLEAR their cart:
   ✓ Call ClearCart() (the current user is identified automatically)

   When user wants to CHECKOUT/PAY:
   ✓ Call InitiatePayment() (the current user is identified automatically)

3. PARAMETER EXTRACTION RULES:
   - Extract values from the user's natural language request
   - User says 'show me laptops' → category = 'Laptop' (get exact spelling from CategoriesList first)
   - User says 'under $1000' → MaxPrice = 1000
   - User says 'silver color' → Color = 'Silver'
   - User says 'Dell laptops under $1500' → filters = ['Category:Laptop', 'MinPrice:0', 'MaxPrice:1500']

4. MULTI-STEP WORKFLOWS:
   For category queries:
   Step 1: Call CategoriesList()
   Step 2: Match user's request to correct spelling
   Step 3: Call GetProductsByCategory(category)

   For complex filters:
   Step 1: Call GetProductProperties() to understand available properties
   Step 2: Map user's query to property names
   Step 3: Call FilterProducts() with all applicable filters

5. IMPORTANT CONSTRAINTS:
   - NEVER make up product information - always call the appropriate tool
   - NEVER skip calling CategoriesList() before GetProductsByCategory()
   - ALWAYS use EXACT category spelling from CategoriesList()
   - For price filters, use MinPrice and MaxPrice (not just 'Price')
   - Pass multiple filters in ONE FilterProducts() call, not multiple calls
   - If a tool result contains an 'error', tell the user briefly what went wrong

6. RESPONSE FORMAT:
   After calling a tool:
   - Wait for the tool result
   - Present the information to the user in a friendly, readable format
   - If the tool returns empty results, tell the user clearly
   - If you need more information from the user, ask specific questions

EXAMPLES OF CORRECT TOOL USAGE:

User: 'Show me laptops'
→ Call CategoriesList()
→ Find 'Laptop' in the list
→ Call GetProductsByCategory('Laptop')

User: 'I want a silver laptop under $1000'
→ Call CategoriesList()
→ Find 'Laptop' in the list
→ Call FilterProducts('Category:Laptop', 'MaxPrice:1000', 'Color:Silver')

User: 'What are the 10 cheapest products?'
→ Call GetCheapestProducts(10)

User: 'Show me all product categories'
→ Call GetAllCategory()

User: 'Clear my cart'
→ Call ClearCart()

Remember: The tools are your only way to access real product data. Use them correctly and completely.";
            return systemPrompt;
        }

        public LLMResponse ParseResponse(JsonElement result)
        {
            var response = new LLMResponse();

            if (!result.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0
                || !candidates[0].TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts))
            {
                _logger.LogWarning("Gemini response had no content: {Response}", result.GetRawText());
                return response;
            }

            response.ModelContent = content.Clone();

            foreach (var part in parts.EnumerateArray())
            {
                // Skip internal reasoning parts from thinking models.
                if (part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True)
                    continue;

                if (part.TryGetProperty("text", out var textProp))
                {
                    response.TextReply += textProp.GetString() ?? "";
                }
                else if (part.TryGetProperty("functionCall", out var functionCall))
                {
                    response.FunctionCalls.Add(new FunctionCall
                    {
                        Name = functionCall.GetProperty("name").GetString() ?? "",
                        Args = functionCall.TryGetProperty("args", out var args) ? args.Clone() : default
                    });
                }
            }
            return response;
        }
    }
}

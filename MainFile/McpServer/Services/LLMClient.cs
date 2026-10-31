using System.Text.Json;
using MCP_Server.Models;

namespace MCP_Server.Services
{
    public class LLMClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        public LLMClient(
        HttpClient http,
        IConfiguration config)
        {
            _httpClient = http;
            _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
            _baseUrl = config["Gemini:BaseUrl"] ?? throw new InvalidOperationException("Gemini BaseUrl not configured");
            _model = config["Gemini:Model"] ?? throw new InvalidOperationException("Gemini Model not configured");

            Console.WriteLine($"[LLMClient Init] API Key present: {!string.IsNullOrWhiteSpace(_apiKey)}");
            Console.WriteLine($"[LLMClient Init] BaseUrl: {_baseUrl}");
            Console.WriteLine($"[LLMClient Init] Model: {_model}");
        }
        public async Task<LLMResponse> ProcessMessageAsync(ChatRequest userRequest, List<object> tools)
        {
            var fullMessage = $"User Name: {userRequest.Name}\nUser Email: {userRequest.Email}\n\nRequest: {userRequest.Message}";

            var systemMessage = SystemMessage();

            var request = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = systemMessage }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = fullMessage } }
                    }
                },
                tools = new[]
                {
                    new { functionDeclarations = tools }
                }
            };

            var url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";

            // Debug: Log request structure
            var requestJson = JsonSerializer.Serialize(request);
            Console.WriteLine($"[Gemini Request] Tools count: {tools.Count}");
            Console.WriteLine($"[Gemini Request] URL: {url}");
            Console.WriteLine($"[Gemini Request] Body (first 500 chars): {requestJson.Substring(0, Math.Min(500, requestJson.Length))}");

            var response = await _httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Gemini Error] Status: {response.StatusCode}");
                Console.WriteLine($"[Gemini Error] Response: {errorContent}");
                throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return ParseResponse(result);
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
   ✓ Call ClearCart(userId) with the user's ID
   
   When user wants to CHECKOUT/PAY:
   ✓ Call InitiatePayment()

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
→ Call ClearCart(userId)

Remember: The tools are your only way to access real product data. Use them correctly and completely.";
            return systemPrompt;
        }

        public LLMResponse ParseResponse(JsonElement result)
        {
            var response = new LLMResponse();

            // Navigate: candidates[0].content.parts
            if (!result.TryGetProperty("candidates", out var candidates))
            {
                Console.WriteLine("[Gemini Response] No candidates found");
                return response;
            }

            if (candidates.GetArrayLength() == 0)
            {
                Console.WriteLine("[Gemini Response] Candidates array is empty");
                return response;
            }

            if (!candidates[0].TryGetProperty("content", out var content))
            {
                Console.WriteLine("[Gemini Response] No content in first candidate");
                return response;
            }

            if (!content.TryGetProperty("parts", out var parts))
            {
                Console.WriteLine("[Gemini Response] No parts in content");
                return response;
            }

            foreach (var part in parts.EnumerateArray())
            {
                // Text response
                if (part.TryGetProperty("text", out var textProp))
                {
                    var textValue = textProp.GetString() ?? "";
                    response.TextReply += textValue;
                    Console.WriteLine($"[Gemini Response] Text: {textValue}");
                }
                // Function call
                else if (part.TryGetProperty("functionCall", out var functionCall))
                {
                    response.ToolUse = functionCall.GetProperty("name").GetString();
                    Console.WriteLine($"[Gemini Response] Tool called: {response.ToolUse}");

                    if (functionCall.TryGetProperty("args", out var args))
                    {
                        response.ToolInput = args;
                        Console.WriteLine($"[Gemini Response] Tool args: {JsonSerializer.Serialize(args)}");
                    }
                }
            }
            return response;
        }

    }
}

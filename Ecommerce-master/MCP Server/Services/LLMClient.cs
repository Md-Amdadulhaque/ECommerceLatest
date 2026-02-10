using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;
using MCP_Server.Models;
using ModelContextProtocol.Protocol;
using static System.Net.WebRequestMethods;

namespace MCP_Server.Services
{
    public class LLMClient
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _services;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        public LLMClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:5000"); // Ollama local API
        }
        public async Task<LLMResponse> PostAsJsonAsync(string userPromt)
        {

            var llmResponse = await _httpClient.PostAsJsonAsync("/predict", new { UserQuery = request.UserQuery });

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

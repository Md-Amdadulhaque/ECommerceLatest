using System.Text.Json;
using System.Xml.Linq;
using MCP_Server.Models;
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
        public LLMClient(
        HttpClient http,
        IServiceProvider services,
        IConfiguration config)
        {
            _httpClient = http;
            _services = services;
            _apiKey = config["Gemini:ApiKey"] ??throw new InvalidOperationException("Gemini API key not configured");
            _baseUrl = config["Gemini:BaseUrl"];
            _model = config["Gemini:Model"];
        }
        public async Task<LLMResponse>
        ProcessMessageAsync(ChatRequest userRequest, List<object> tools)
        {
            var fullMessage = $"User Name: {userRequest.Name}\nUser Email: {userRequest.Email}\n\nRequest: {userRequest.Message}";

            var request = new
            {
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
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return ParseResponse(result);
        }
        public LLMResponse ParseResponse(JsonElement result)
        {
            var response = new LLMResponse();

            // Navigate: candidates[0].content.parts
            if (!result.TryGetProperty("candidates", out var candidates))
                return response;

            if (candidates.GetArrayLength() == 0)
                return response;

            if (!candidates[0].TryGetProperty("content", out var content))
                return response;

            if (!content.TryGetProperty("parts", out var parts))
                return response;

            foreach (var part in parts.EnumerateArray())
            {
                // Text response
                if (part.TryGetProperty("text", out var textProp))
                {
                    response.TextReply += textProp.GetString() ?? "";
                }
                // Function call - NOT "tool_use"!
                else if (part.TryGetProperty("functionCall", out var functionCall))
                {
                    response.ToolUse = functionCall.GetProperty("name").GetString();

                    if (functionCall.TryGetProperty("args", out var args))
                    {
                        response.ToolInput = args;
                    }
                }
            }

            return response;
        }

    }
}

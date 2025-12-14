using MCP_Server.Helpers;
using MCP_Server.Tools;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCP_Server.Services;

namespace MCP_Server.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class Chatv2Controller : ControllerBase
    {
        private readonly McpToolInvoker _invoker;
        private readonly SourceProjectTools _tools;
        private readonly HttpClient _httpClient;
        private readonly IToolService _toolService;

        public Chatv2Controller(McpToolInvoker invoker, SourceProjectTools tools, IHttpClientFactory httpClientFactory,IToolService toolService)
        {
            _invoker = invoker;
            _tools = tools;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:11434/");
            _toolService = toolService;
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] ChatRequest1 request)
        {
            if (string.IsNullOrEmpty(request.UserQuery))
                return BadRequest(new { error = "UserQuery is required" });

            string prompt = ToolPromptBuilder.BuildPrompt(_toolService, request.UserQuery);

            var llmResponseMsg = await _httpClient.PostAsJsonAsync("/api/generate", new
            {
                model = "llama2",
                prompt = prompt,
                stream = false,
                format = "json",
                options = new
                {
                    temperature = 0.1,
                    num_predict = 300
                }
            });

            var resultJson = await llmResponseMsg.Content.ReadAsStringAsync();

            if (!llmResponseMsg.IsSuccessStatusCode)
            {
                return StatusCode(500, new { error = "LLM API error", details = resultJson });
            }

            // Step 1: Deserialize the wrapper response
            var llmResult = JsonSerializer.Deserialize<LlmResponse>(resultJson);

            if (llmResult == null || string.IsNullOrWhiteSpace(llmResult.response))
            {
                return StatusCode(500, new { error = "LLM returned empty response", raw = resultJson });
            }

            try
            {
                // Step 2: Extract the tool JSON from 'response'
                var toolJson = llmResult.response.Trim();

                // Optional: fix incomplete JSON from LLM
                //toolJson = ToolPromptBuilder.CleanJsonResponse(toolJson);

                // Step 3: Deserialize to ToolCall
                var toolCall = JsonSerializer.Deserialize<ToolCall>(toolJson);

                if (toolCall == null || string.IsNullOrWhiteSpace(toolCall.Tool))
                {
                    return StatusCode(500, new { error = "Failed to parse tool call", response = toolJson });
                }

                // Return structured response
                return Ok(new
                {
                    success = true,
                    tool = toolCall.Tool,
                    parameters = toolCall.Parameters
                });
            }
            catch (JsonException ex)
            {
                return StatusCode(500, new
                {
                    error = "JSON parsing failed",
                    details = ex.Message,
                    llmResponse = llmResult.response
                });
            }
        }
    }

    // -----------------------------
    // Models
    // -----------------------------
    public class ToolCall
    {
        [JsonPropertyName("Tool")]
        public string Tool { get; set; }

        [JsonPropertyName("Parameters")]
        public Dictionary<string, JsonElement> Parameters { get; set; } = new Dictionary<string, JsonElement>();

        public string GetString(string key)
        {
            if (!Parameters.ContainsKey(key)) return null;
            var element = Parameters[key];
            return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        }

        public string[] GetStringArray(string key)
        {
            if (!Parameters.ContainsKey(key)) return null;
            var element = Parameters[key];
            return element.ValueKind == JsonValueKind.Array
                ? element.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()).ToArray()
                : null;
        }

        public int? GetInt(string key)
        {
            if (!Parameters.ContainsKey(key)) return null;
            var element = Parameters[key];

            if (element.ValueKind == JsonValueKind.Number) return element.GetInt32();
            if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out int result)) return result;

            return null;
        }
    }

    public class ChatRequest1
    {
        public string UserQuery { get; set; } = string.Empty;
    }

    public class LlmResponse
    {
        public string model { get; set; }
        public DateTime created_at { get; set; }
        public string response { get; set; }
        public bool done { get; set; }

        // Optional metrics
        public long? total_duration { get; set; }
        public long? load_duration { get; set; }
        public int? prompt_eval_count { get; set; }
        public long? prompt_eval_duration { get; set; }
        public int? eval_count { get; set; }
        public long? eval_duration { get; set; }
    }
}

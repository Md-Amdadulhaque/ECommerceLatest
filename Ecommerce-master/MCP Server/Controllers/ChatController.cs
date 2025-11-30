using System.Text.Json;
using MCP_Server.Models;
using MCP_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMcpServer.Services;

namespace MCP_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly LLMClient _llmClient;
        private readonly ToolExecutor _toolExecutor;
        public ChatController(LLMClient llmClient,ToolExecutor toolExecutor)
        {
            _llmClient = llmClient;
            _toolExecutor = toolExecutor;
        }
        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            // Define tools Claude can use
            var tools = GetToolDefinitions();

            var llmResponse = await _llmClient
                .ProcessMessageAsync(request, tools);

            if (llmResponse.ToolUse != null)
            {
                var toolName = llmResponse.ToolUse;
                var toolInput = llmResponse.ToolInput;

                // Step 3: Execute the tool (call Source Project API)
                var toolResult = await _toolExecutor.ExecuteAsync(toolName, toolInput);
                return Ok(new ChatResponse
                {
                    ToolUsed = toolResult?.ToolUsed,
                    ToolResult = toolResult?.Result
                });
            }
            return new ChatResponse
            {
                Reply = "No ToolFound"
            };
        }
        private List<object> GetToolDefinitions()
        {

            return new List<object>
{
    new
    {
        name = "GetCustomer",
        description = "Get customer information by User Name and User Email",
        parameters = new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string", description = "The Customer Name" },
                email = new { type = "string", description = "The Customer Email" }
            },
            required = new[] { "name", "email" }
        }
    }
};
        }
    }
}

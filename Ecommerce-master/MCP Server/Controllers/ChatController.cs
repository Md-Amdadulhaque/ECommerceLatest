using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;
using MCP_Server.Models;
using MCP_Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyMcpServer.Services;
using MCP_Server.Tools;
using ModelContextProtocol.Protocol;
using System.Net.Http;

namespace MCP_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly LLMClient _llmClient;
        private readonly ToolExecutor _toolExecutor;
        private readonly IToolService _toolService;
        private readonly SourceProjectTools _sourceProjectTools;
        public ChatController(LLMClient llmClient, ToolExecutor toolExecutor, IToolService toolService,SourceProjectTools sourceProjectTools)
        {
            _llmClient = llmClient;
            _toolExecutor = toolExecutor;
            _toolService = toolService;
            _sourceProjectTools = sourceProjectTools;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            // Auto-discover tools from ToolService

            string prompt = ToolPromptBuilder.BuildPrompt(_sourceProjectTools, request);

            // var toolResult = await _toolExecutor.ExecuteAsync("GetProductsByCategory", { "Laptop"});

              var llmResponse = await _llmClient.PostAsJsonAsync(prompt);


            if (!llmResponseMsg.IsSuccessStatusCode)
                return StatusCode(500, new { error = "LLM API error" });


            if (llmResponse.ToolUse != null)
            {
                var toolName = llmResponse.ToolUse;
                var toolInput = llmResponse.ToolInput;
                var toolResult = await _toolExecutor.ExecuteAsync(toolName, toolInput);
                return Ok(new ChatResponse
                {
                    ToolUsed = toolResult?.ToolUsed,
                    ToolResult = toolResult?.Result,
                    Reply = "Found Product"
                });
              }
                return Ok(new ChatResponse
                {
                    Reply = "No ToolFound"
                });
        }
    }
}

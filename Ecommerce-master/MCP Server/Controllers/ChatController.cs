using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;
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

        public ChatController(LLMClient llmClient, ToolExecutor toolExecutor)
        {
            _llmClient = llmClient;
            _toolExecutor = toolExecutor;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            // Auto-discover tools from [McpServerToolType] classes
            var tools = GetToolDefinitions();

            var llmResponse = await _llmClient
                .ProcessMessageAsync(request, tools);

            if (llmResponse.ToolUse != null)
            {
                var toolName = llmResponse.ToolUse;
                var toolInput = llmResponse.ToolInput;
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
            var tools = new List<object>();
            // Scan all assemblies for types marked with [McpServerToolType]
            var toolTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetCustomAttribute<McpServerToolType>() != null);

            foreach (var toolType in toolTypes)
            {
                // Find all methods marked with [McpServerTool]
                var methods = toolType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.GetCustomAttribute<McpServerTool>() != null);

                foreach (var method in methods)
                {
                    var description = method.GetCustomAttribute<Description>()?.Description ?? "No description";
                    var parameters = method.GetParameters();

                    var toolDef = new
                    {
                        name = method.Name,
                        description = description,
                        parameters = new
                        {
                            type = "object",
                            properties = BuildParameterProperties(parameters),
                            required = parameters
                                .Where(p => !p.HasDefaultValue && p.Name != "client")
                                .Select(p => p.Name)
                                .ToArray()
                        }
                    };
                    tools.Add(toolDef);
                }
            }

            return tools;
        }

        private Dictionary<string, object> BuildParameterProperties(ParameterInfo[] parameters)
        {
            var props = new Dictionary<string, object>();

            foreach (var param in parameters)
            {
                // Skip SourceClient parameter
                if (param.ParameterType.Name == "SourceClient") continue;

                var description = param.GetCustomAttribute<Description>()?.Description ?? param.Name;
                var typeName = param.ParameterType.Name.ToLower();

                var jsonType = typeName switch
                {
                    "int32" or "int64" => "integer",
                    "string" => "string",
                    "boolean" => "boolean",
                    "double" or "decimal" => "number",
                    _ => "string"
                };
                props[param.Name] = new
                {
                    type = jsonType,
                    description = description
                };
            }
            return props;
        }
    }
}

using System.Reflection;
using System.Text.Json;
using MCP_Server.Models;
using MCP_Server.Services;
using MCP_Server.Tools;
using ModelContextProtocol.Server;

namespace MyMcpServer.Services;

public class ToolExecutor
{
    private readonly SourceProjectTools _sourceProjectTools;
    private readonly ILogger<ToolExecutor> _logger;

    public ToolExecutor(SourceProjectTools sourceProjectTools, ILogger<ToolExecutor> logger)
    {
        _sourceProjectTools = sourceProjectTools;
        _logger = logger;
    }

    // Runs any [McpServerTool] method by name. Errors are returned as data (not thrown)
    // so the LLM can see what went wrong and recover or explain it to the user.
    // userId comes from the caller's request, never from the LLM, so a prompt
    // cannot make the agent act on another user's cart.
    public async Task<ToolResult> ExecuteAsync(string toolName, JsonElement input, string? userId)
    {
        var method = typeof(SourceProjectTools).GetMethod(toolName, BindingFlags.Public | BindingFlags.Instance);
        if (method == null || method.GetCustomAttribute<McpServerToolAttribute>() == null)
            return Error(toolName, $"Unknown tool '{toolName}'");

        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];

            if (ToolService.IsContextParameter(p))
            {
                if (string.IsNullOrEmpty(userId))
                    return Error(toolName, "This action needs a logged-in user, but no userId was provided");
                args[i] = userId;
            }
            else if (input.ValueKind == JsonValueKind.Object && input.TryGetProperty(p.Name!, out var val))
            {
                try
                {
                    args[i] = val.Deserialize(p.ParameterType);
                }
                catch (JsonException)
                {
                    return Error(toolName, $"Parameter '{p.Name}' must be of type {p.ParameterType.Name}");
                }
            }
            else if (p.HasDefaultValue)
            {
                args[i] = p.DefaultValue;
            }
            else if (p.ParameterType.IsArray)
            {
                args[i] = Array.CreateInstance(p.ParameterType.GetElementType()!, 0);
            }
            else
            {
                return Error(toolName, $"Missing required parameter '{p.Name}'");
            }
        }

        try
        {
            var task = (Task)method.Invoke(_sourceProjectTools, args)!;
            await task;
            var result = task.GetType().GetProperty("Result")?.GetValue(task);
            return new ToolResult { ToolUsed = toolName, Result = result };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool {Tool} failed", toolName);
            return Error(toolName, $"Tool failed: {ex.Message}");
        }
    }

    private static ToolResult Error(string toolName, string message) =>
        new() { ToolUsed = toolName, Result = new { error = message } };
}

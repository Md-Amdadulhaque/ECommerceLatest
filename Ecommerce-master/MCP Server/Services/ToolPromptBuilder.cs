using System.ComponentModel;
using System.Reflection;
using System.Text;
using MCP_Server.Models;
using ModelContextProtocol.Server;

public static class ToolPromptBuilder
{
    public static string BuildPrompt(object toolsInstance, ChatRequest userQuery)
    {
        var type = toolsInstance.GetType();
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance) .Where(m => m.GetCustomAttribute(typeof(McpServerToolAttribute)) != null);

        var sb = new StringBuilder();
        sb.AppendLine("You are a tool-mapper. The user sends a query in natural language.");
        sb.AppendLine("Available tools:");

        foreach (var method in methods)
        {
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
            var desc = descAttr != null ? descAttr.Description : "";
            var paramList = string.Join(", ", method.GetParameters().Select(p => p.Name));
            sb.AppendLine($"- {method.Name}({paramList}): {desc}");
        }
        sb.AppendLine();
        sb.AppendLine($"User request: \"{userQuery.Message + userQuery.Name+userQuery.Email}\"");
        sb.AppendLine(@"Return strictly JSON in this format:{""tool"": ""<tool_name>"",""parameters"": { ""<param>"" : ""<value>"", ... }}");
        return sb.ToString();
    }
}

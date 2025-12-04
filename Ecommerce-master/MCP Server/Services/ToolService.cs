using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.Linq;

namespace MCP_Server.Services
{
    public interface IToolService
    {
        List<object> GetToolDefinitions();
    }

    public class ToolService : IToolService
    {
        public List<object> GetToolDefinitions()
        {
            var tools = new List<object>();

            var toolTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetCustomAttribute<McpServerToolType>() != null);

            foreach (var toolType in toolTypes)
            {
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

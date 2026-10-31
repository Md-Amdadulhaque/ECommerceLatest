using System.ComponentModel;
using System.Reflection;
using MCP_Server.Models;
using ModelContextProtocol.Server;

namespace MCP_Server.Services
{
    public interface IToolService
    {
        List<ToolDefinition> GetToolDefinitions();
    }

    // Builds tool definitions from every [McpServerTool] method, so adding a new tool
    // is just adding a method with [McpServerTool] and [Description] attributes.
    public class ToolService : IToolService
    {
        private readonly ILogger<ToolService> _logger;
        private readonly Lazy<List<ToolDefinition>> _tools;

        public ToolService(ILogger<ToolService> logger)
        {
            _logger = logger;
            _tools = new Lazy<List<ToolDefinition>>(DiscoverTools);
        }

        public List<ToolDefinition> GetToolDefinitions() => _tools.Value;

        // Parameters filled in by the server from the request, hidden from the LLM.
        public static bool IsContextParameter(ParameterInfo p) =>
            string.Equals(p.Name, "userId", StringComparison.OrdinalIgnoreCase);

        private List<ToolDefinition> DiscoverTools()
        {
            var tools = new List<ToolDefinition>();

            var toolTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<McpServerToolTypeAttribute>() != null);

            foreach (var toolType in toolTypes)
            {
                var methods = toolType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);

                foreach (var method in methods)
                {
                    var parameters = method.GetParameters()
                        .Where(p => !IsContextParameter(p))
                        .ToArray();

                    tools.Add(new ToolDefinition
                    {
                        Name = method.Name,
                        Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? method.Name,
                        Parameters = parameters.Length == 0 ? null : new ToolParameters
                        {
                            Properties = parameters.ToDictionary(p => p.Name!, BuildProperty),
                            Required = parameters.Where(p => !p.HasDefaultValue).Select(p => p.Name!).ToArray()
                        }
                    });

                    _logger.LogInformation("Discovered tool: {Tool} with {ParamCount} parameters", method.Name, parameters.Length);
                }
            }

            return tools;
        }

        private static ToolProperty BuildProperty(ParameterInfo param)
        {
            var property = ToSchema(param.ParameterType);
            property.Description = param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? param.Name;
            return property;
        }

        private static ToolProperty ToSchema(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsArray)
                return new ToolProperty { Type = "array", Items = ToSchema(type.GetElementType()!) };

            return new ToolProperty
            {
                Type = Type.GetTypeCode(type) switch
                {
                    TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => "integer",
                    TypeCode.Single or TypeCode.Double or TypeCode.Decimal => "number",
                    TypeCode.Boolean => "boolean",
                    _ => "string"
                }
            };
        }
    }
}

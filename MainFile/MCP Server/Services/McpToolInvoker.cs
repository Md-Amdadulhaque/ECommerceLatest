using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MCP_Server.Tools;

namespace MCP_Server.Helpers
{
    public class McpToolInvoker
    {
        private readonly SourceProjectTools _tools;

        public McpToolInvoker(SourceProjectTools tools)
        {
            _tools = tools;
        }

        public async Task<object?> InvokeAsync(string toolName, Dictionary<string, string> userParams)
        {
            var method = typeof(SourceProjectTools)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => string.Equals(m.Name, toolName, StringComparison.OrdinalIgnoreCase));

            if (method == null) throw new Exception($"Tool '{toolName}' not found");

            if (toolName.Equals("FilterProducts", StringComparison.OrdinalIgnoreCase))
            {
                var filters = userParams.Select(kv => $"{kv.Key}:{kv.Value}").ToArray();
                return await (Task<object?>)method.Invoke(_tools, new object[] { filters });
            }

            var parameters = method.GetParameters();
            var paramValues = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (userParams.TryGetValue(param.Name!, out var value))
                    paramValues[i] = Convert.ChangeType(value, param.ParameterType);
                else if (param.HasDefaultValue)
                    paramValues[i] = param.DefaultValue;
                else
                    throw new Exception($"Missing required parameter '{param.Name}'");
            }

            var task = (Task)method.Invoke(_tools, paramValues)!;
            await task.ConfigureAwait(false);
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }
    }
}
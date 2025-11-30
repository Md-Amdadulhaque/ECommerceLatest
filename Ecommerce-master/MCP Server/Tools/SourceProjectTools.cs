using System.ComponentModel;
using ModelContextProtocol.Server;
using MCP_Server.Services;
namespace MCP_Server.Tools
{
    [McpServerToolType]
    public class SourceProjectTools
    {
        [McpServerTool]
        [Description("Get customer information by ID")]
        public static async Task<string> GetCustomer(
        SourceClient client,
        [Description("The customer ID")] string customerId)
        {
            var result = await client.GetAsync<object>($"/api/customers/{customerId}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }
    }
}

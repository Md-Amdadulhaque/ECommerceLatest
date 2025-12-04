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

        [McpServerTool]
        [Description("Get top cheapest products from database")]
        public static async Task<string> GetCheapestProducts(
            SourceClient client,
            [Description("Number of cheapest products to return")] int count = 5)
        {
            var result = await client.GetAsync<object>($"/api/product/cheapest?count={count}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }
    }
}

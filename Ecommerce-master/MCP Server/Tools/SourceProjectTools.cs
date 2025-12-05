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

        // [McpServerTool]
        // [Description("Get all laptops from database")]
        // public static async Task<string> GetAllLaptops(
        //     SourceClient client)
        // {
        //     var result = await client.GetAsync<object>("/api/products?category=laptop");
        //     return System.Text.Json.JsonSerializer.Serialize(result);
        // }

        // [McpServerTool]
        // [Description("Get all Mac laptops from database")]
        // public static async Task<string> GetMacLaptops(
        //     SourceClient client)
        // {
        //     var result = await client.GetAsync<object>("/api/products?category=mac");
        //     return System.Text.Json.JsonSerializer.Serialize(result);
        // }

        // [McpServerTool]
        // [Description("Get all mobile phones from database")]
        // public static async Task<string> GetAllMobiles(
        //     SourceClient client)
        // {
        //     var result = await client.GetAsync<object>("/api/products?category=mobile");
        //     return System.Text.Json.JsonSerializer.Serialize(result);
        // }

        [McpServerTool]
        [Description("Get products by category (laptop, mac, mobile, etc)")]
        public static async Task<string> GetProductsByCategory(
            SourceClient client,
            [Description("Category name (e.g., laptop, mac, mobile)")] string category)
        {
            var result = await client.GetAsync<object>($"/api/product/category/{category}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }
    }
}

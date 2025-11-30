using System.Text.Json;
using MCP_Server.Services;
using MCP_Server.Models;
using ModelContextProtocol.Protocol;
namespace MyMcpServer.Services;

public static class ObjectExtensions
{
    public static T? ToModel<T>(this object? obj)
    {
        if (obj == null) return default;

        if (obj is T typed)
            return typed;

        if (obj is JsonElement json)
            return json.Deserialize<T>();

        var jsonString = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}
public class ToolExecutor
{
    private readonly SourceClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    public ToolExecutor(SourceClient client)
    {
        _client = client;
    }
    
    public async Task<ToolResult?> ExecuteAsync(string toolName, JsonElement input)
    {
            object? result = toolName switch
            {
                "GetCustomer" => await GetCustomer(input),
                "GetCustomerOrders" => await GetCustomerOrders(input),
                "CreateOrder" => await CreateOrder(input),
                _ => new { error = $"Unknown tool: {toolName}" }
            };
            return result.ToModel<ToolResult>();
    }

    private async Task<Object> GetCustomer(JsonElement input)
    {
        var customerName = input.GetProperty("name").GetString();
        var customerEmail = input.GetProperty("email").GetString();
        var result = await _client.GetDataByModelAsync<object>($"/api/GetUserByUser", new
        {Name = customerName,
        Email = customerEmail});
        return JsonSerializer.Serialize(result);
    }

    private async Task<string> GetCustomerOrders(JsonElement input)
    {
        var customerId = input.GetProperty("customerId").GetString();
        var result = await _client.GetAsync<object>($"/api/customers/{customerId}/orders");
        return JsonSerializer.Serialize(result);
    }

    private async Task<string> CreateOrder(JsonElement input)
    {
        var order = new
        {
            customerId = input.GetProperty("customerId").GetString(),
            product = input.GetProperty("product").GetString(),
            quantity = input.GetProperty("quantity").GetInt32()
        };
        var result = await _client.PostAsync<object>("/api/orders", order);
        return JsonSerializer.Serialize(result);
    }
}
using System.ComponentModel;
using ModelContextProtocol.Server;
using MCP_Server.Services;
using System.Text.Json;
namespace MCP_Server.Tools
{
    [McpServerToolType]
    public class SourceProjectTools
    {

        private readonly SourceClient _client;

        public SourceProjectTools(SourceClient sourceClient)
        {
            _client = sourceClient;
        }
        [McpServerTool]
        [Description("Get customer information by ID")]
        public async Task<string> GetCustomer(
            [Description("The customer ID")] string customerId)
        {
            var result = await _client.GetAsync<object>($"/api/customers/{customerId}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }

        [McpServerTool]
        [Description("Get top cheapest products from database")]
        public async Task<object?> GetCheapestProducts(
            [Description("Number of cheapest products to return")] int count = 5)
        {
            var result = await _client.GetAsync<object>($"/api/product/cheapest?count={count}");
            return System.Text.Json.JsonSerializer.Serialize(result);
        }

        [McpServerTool]
        [Description("Get products by category (laptop, mac, mobile, etc)")]
        public async Task<object?> GetProductsByCategory(
            [Description("Category name (e.g., laptop, mac, mobile)")] string category)
        {
            var result = await _client.GetAsync<object>($"/api/product/category/{category}");
            return result;
        }

        [McpServerTool]
        [Description("InitiatePayment")]
        public async Task<object?> InitaitePayment()
        {
            var result = await _client.GetAsync<object>($"/api/product/category");
            return result;
        }


        [McpServerTool]
        [Description("InitiatePayment")]
        public async Task<object?> GetProductsByCategory()
        {
            var result = await _client.GetAsync<object>($"/api/product/category");
            return result;
        }


        [McpServerTool]
        [Description("Get all categori from Api")]
        public async Task<object?> GetAllCategory()
        {
            var result = await _client.GetAsync<object>($"/api/Category");
            return result;
        }

        [McpServerTool]
        [Description("Clear User Cart")]
        public async Task<object?> ClearCart(string userName, string email)
        {
            var result = await _client.GetAsync<object>($"/api/Category");
            return result;
        }

        [McpServerTool]
        [Description("Get list of all available product categories")]
        public Task<string> CategoriesList()
        {
            var categories = new List<string>
            {
                "laptop",
                "mac",
                "mobile",
                "tablet",
                "accessories"
            };
            return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(categories));
        }

        [McpServerTool]
        [Description("Filter products using dynamic criteria. Pass filters as 'property:value' pairs.")]
        public async Task<object?> FilterProducts(
        [Description("Array of filter criteria in 'property:value' of my product model property" +
            " format. Examples: 'category:laptop', 'brand:Dell', 'minPrice:500', 'maxPrice:1500', 'color:Silver'")]
        params string[] filters)
        {
            var filterRequest = ParseToFilterRequest(filters);

            var result = await _client.PostAsJsonAsync<object>("/api/product/filter", JsonContent.Create(filterRequest));
            
            return result;
        }

        private ProductFilterRequest ParseToFilterRequest(string[] filters)
        {
            var request = new ProductFilterRequest();

            foreach (var filter in filters)
            {
                var parts = filter.Split(':', 2);
                if (parts.Length != 2) continue;

                var propertyName = parts[0].Trim();
                var value = parts[1].Trim();

                switch (propertyName)
                {
                    case "Name":
                        request.Name = value;
                        break;
                    case "Category":
                        request.Category = value;
                        break;
                    case "MinPrice":
                        if (int.TryParse(value, out var minPrice))
                            request.MinPrice = minPrice;
                        break;
                    case "MaxPrice":
                        if (int.TryParse(value, out var maxPrice))
                            request.MaxPrice = maxPrice;
                        break;
                    case "Color":
                        request.Color = value;
                        break;
                    case "NumberOfItemAvaiable":
                        if (int.TryParse(value, out var inStock))
                            request.NumberOfItemAvaiable = inStock;
                        break;
                }
            }

            return request;
        }
        public class ProductFilterRequest
        {
            public string? Category { get; set; }
            public int? MinPrice { get; set; }
            public int? MaxPrice { get; set; }
            public string? Color { get; set; }
            public int? NumberOfItemAvaiable { get; set; }
            public string? Name { get; set; }
        }

        [McpServerTool]
        [Description("Get available ProductFilter properties and their details to help map user queries to filterable fields. Use property Name to map")]
        public async Task<object?> GetProductProperties()
        {
            return new
            {
                properties = new object[]
                {
            new {
                name = "Name",
                type = "string",
                nullable = false,
                description = "Product name",
                searchable = true,
                examples = new[] { "MacBook Pro", "Dell XPS 15", "iPhone 15" }
            },
            new {
                name = "MinPrice",
                type = "integer",
                nullable = true,
                description = "Product price",
                filterable = true,
                supportsRange = true
            },
             new {
                name = "MaxPrice",
                type = "integer",
                nullable = true,
                description = "Product price",
                filterable = true,
                supportsRange = true
            },
            new {
                name = "Category",
                type = "string",
                nullable = false,
                description = "Product category",
                filterable = true,
                examples = new[] { "laptop", "mobile", "tablet", "accessories" }
            },
            new {
                name = "Color",
                type = "string",
                nullable = true,
                description = "Product color",
                filterable = true,
                examples = new[] { "Silver", "Black", "White", "Gold", "Blue" }
            }
           },
                filterMapping = new
                {
                    byText = new[] { "Name",},
                    byCategory = new[] { "Category" },
                    byPriceRange = new[] { "MinPrice","MaxPrice" },
                    byColor = new[] { "Color" },
                }
            };
        }

    }

}

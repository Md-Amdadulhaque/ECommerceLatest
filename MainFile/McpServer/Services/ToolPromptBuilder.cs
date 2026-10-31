using MCP_Server.Controllers;

namespace MCP_Server.Helpers
{
    using MCP_Server.Services;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="ToolPromptBuilder" />
    /// </summary>
    public static class ToolPromptBuilder
    {
        /// <summary>
        /// The BuildPrompt
        /// </summary>
        /// <param name="toolService">The toolService<see cref="IToolService"/></param>
        /// <param name="userQuery">The userQuery<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string BuildPrompt(IToolService toolService, ChatRequest1 request)
        {
            // Step 1: Get all tools
            var toolDefs = toolService.GetToolDefinitions();
            var sb = new StringBuilder();

            // Step 2: Instructions
            sb.AppendLine("You are a tool selection assistant. Your ONLY job is to return JSON.");
            sb.AppendLine("Map the user query to a tool. Return ONLY valid JSON. NO extra text.");
            sb.AppendLine("The JSON format must be:");
            sb.AppendLine(@"
            {
            ""Tool"": ""<ToolName>"",
            ""Parameters"": {
            ""param1"": ""value"",
            ""param2"": [""value1"", ""value2""]
            }}");

            sb.AppendLine();

            sb.AppendLine("Available tools and parameters:");

            // Step 3: Include tool definitions
            foreach (var tool in toolDefs)
            {
                string toolName = tool.Name;

                sb.Append($"- {toolName}");

                var props = tool.Parameters?.Properties;
                if (props is { Count: > 0 })
                {
                    var paramList = props.Select(kvp => $"{kvp.Key}: {kvp.Value.Description ?? kvp.Key}");
                    sb.Append($"({string.Join(", ", paramList)})");
                }
                else
                {
                    sb.Append("(no parameters)");
                }

                sb.AppendLine($" - {tool.Description}");
                if (toolName == "CategoriesList")
                {
                    sb.AppendLine(
                        "Laptop, Mac, Mobile, Tablet, Accessories, Books, Electronics, Cycle, Furniture, Sports, Toys, Footwear, Gaming");
                }

                if (toolName == "GetProductProperties")
                {
                    string propertyDetails = """
                        {
                        "properties": [
                        {
                        "name": "Name",
                        "type": "string",
                        "nullable": false,
                        "description": "Product name",
                        "searchable": true,
                        "examples": ["MacBook Pro", "Dell XPS 15", "iPhone 15"]
                        },
                        {
                        "name": "MinPrice",
                        "type": "integer",
                        "nullable": true,
                        "description": "Product price",
                        "filterable": true,
                        "supportsRange": true
                        },
                        {
                        "name": "MaxPrice",
                        "type": "integer",
                        "nullable": true,
                        "description": "Product price",
                        "filterable": true,
                        "supportsRange": true
                        },
                        {
                        "name": "Category",
                        "type": "string",
                        "nullable": false,
                        "description": "Product category",
                        "filterable": true,
                        "examples": ["laptop", "mobile", "tablet", "accessories"]
                        },
                        {
                        "name": "Color",
                        "type": "string",
                        "nullable": true,
                        "description": "Product color",
                        "filterable": true,
                        "examples": ["Silver", "Black", "White", "Gold", "Blue"]
                        }
                        ],
                        "filterMapping": {
                        "byText": ["Name"],
                        "byCategory": ["Category"],
                        "byPriceRange": ["MinPrice", "MaxPrice"],
                        "byColor": ["Color"]
                        }
                        }
                        """;
                    sb.AppendLine("When you will get multiple filterable property then map property with this");
                    sb.AppendLine(propertyDetails);
                }
            }

            sb.AppendLine();
            sb.AppendLine("Examples:");

            foreach (var tool in toolDefs)
            {
                string exampleParams = "{}";

                var props = tool.Parameters?.Properties;
                if (props is { Count: > 0 })
                {
                    var paramPairs = props.Keys.Select(key => $"\"{key}\":\"<value_for_{key}>\"");
                    exampleParams = "{" + string.Join(",", paramPairs) + "}";
                }

                sb.AppendLine($@"""example query for {tool.Name}"" -> {{""Tool"":""{tool.Name}"",""Parameters"":{exampleParams}}}");
            }

            sb.AppendLine();
            // After tool definitions, add parameter extraction guidance
            sb.AppendLine("PARAMETER EXTRACTION RULES:");
            sb.AppendLine("- Price mentions (\"under $X\", \"below $X\", \"less than $X\") → MaxPrice: X");
            sb.AppendLine("- Price mentions (\"over $X\", \"above $X\", \"more than $X\") → MinPrice: X");
            sb.AppendLine("- Price range (\"between $X and $Y\") → MinPrice: X, MaxPrice: Y");
            sb.AppendLine("- Product names → Name parameter");
            sb.AppendLine("- Colors mentioned → Color parameter");
            sb.AppendLine("- Category keywords (laptop, phone, tablet, etc.) → Category parameter");
            sb.AppendLine();

            sb.AppendLine($@"""User query: {request.UserQuery}"" -> ");

            return sb.ToString();
        }
    }

}

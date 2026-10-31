using System.Text.Json.Serialization;

namespace MCP_Server.Models
{
    // JSON-schema style tool description sent to the LLM (Gemini functionDeclarations).
    public class ToolDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        // Gemini rejects an "object" schema with no properties, so tools without
        // parameters leave this out entirely.
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ToolParameters? Parameters { get; set; }
    }

    public class ToolParameters
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        [JsonPropertyName("properties")]
        public Dictionary<string, ToolProperty> Properties { get; set; } = new();

        [JsonPropertyName("required")]
        public string[] Required { get; set; } = [];
    }

    public class ToolProperty
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "string";

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonPropertyName("items")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ToolProperty? Items { get; set; }
    }
}

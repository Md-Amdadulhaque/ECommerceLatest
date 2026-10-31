namespace MCP_Server.Models
{
    public class ChatResponse
    {
        public string? Reply { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public List<string> ToolsUsed { get; set; } = new();
    }
}

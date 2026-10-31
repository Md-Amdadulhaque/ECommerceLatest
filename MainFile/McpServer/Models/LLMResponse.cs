using System.Text.Json;

namespace MCP_Server.Models
{
    public class LLMResponse
    {
        public string TextReply { get; set; } = "";

        // Gemini can ask for several tools in one turn (parallel function calls).
        public List<FunctionCall> FunctionCalls { get; set; } = new();

        // The model's turn exactly as returned, sent back on the next request so the
        // conversation history stays valid (it can include thought signatures).
        public JsonElement ModelContent { get; set; }
    }

    public class FunctionCall
    {
        public string Name { get; set; } = "";
        public JsonElement Args { get; set; }
    }
}

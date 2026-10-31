using MCP_Server.Models;
using MyMcpServer.Services;

namespace MCP_Server.Services
{
    // The agent loop: ask the model, run the tools it asks for, send the results back,
    // and repeat until it answers in text (or the step limit is hit).
    public class AgentService
    {
        private const int MaxSteps = 6;

        private readonly LLMClient _llmClient;
        private readonly ToolExecutor _toolExecutor;
        private readonly IToolService _toolService;
        private readonly ILogger<AgentService> _logger;

        public AgentService(LLMClient llmClient, ToolExecutor toolExecutor, IToolService toolService, ILogger<AgentService> logger)
        {
            _llmClient = llmClient;
            _toolExecutor = toolExecutor;
            _toolService = toolService;
            _logger = logger;
        }

        public async Task<ChatResponse> ChatAsync(ChatRequest request)
        {
            var tools = _toolService.GetToolDefinitions();
            var contents = new List<object>
            {
                LLMClient.UserText($"User Name: {request.Name}\nUser Email: {request.Email}\n\nRequest: {request.Message}")
            };
            var response = new ChatResponse { ConversationId = request.ConversationId };

            for (int step = 1; step <= MaxSteps; step++)
            {
                var llmResponse = await _llmClient.GenerateAsync(contents, tools);

                if (llmResponse.FunctionCalls.Count == 0)
                {
                    response.Reply = string.IsNullOrWhiteSpace(llmResponse.TextReply)
                        ? "Sorry, I couldn't come up with an answer. Please try rephrasing."
                        : llmResponse.TextReply;
                    return response;
                }

                contents.Add(llmResponse.ModelContent);

                var results = new List<(string Name, object? Result)>();
                foreach (var call in llmResponse.FunctionCalls)
                {
                    var result = await _toolExecutor.ExecuteAsync(call.Name, call.Args, request.UserId);
                    _logger.LogInformation("Step {Step}: {Tool}({Args})", step, call.Name, call.Args.ValueKind == default ? "" : call.Args.GetRawText());
                    response.ToolsUsed.Add(call.Name);
                    results.Add((call.Name, result.Result));
                }

                contents.Add(LLMClient.ToolResults(results));
            }

            _logger.LogWarning("Agent stopped after {MaxSteps} steps without a final answer", MaxSteps);
            response.Reply = "Sorry, that request needed too many steps. Please try a simpler question.";
            return response;
        }
    }
}

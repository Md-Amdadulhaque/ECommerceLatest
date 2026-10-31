using MCP_Server.Models;
using MCP_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCP_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AgentService _agent;

        public ChatController(AgentService agent)
        {
            _agent = agent;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message is required" });

            try
            {
                return Ok(await _agent.ChatAsync(request));
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "The AI service is unavailable right now", details = ex.Message });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = "The AI service took too long to respond" });
            }
        }
    }
}

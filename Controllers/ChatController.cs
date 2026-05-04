using AIChatBot.Models;
using AIChatBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIChatBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chat;

        public ChatController(IChatService chat)
        {
            _chat = chat;
        }

        [HttpPost("new")]
        public async Task<IActionResult> NewSession([FromBody] CreateSessionRequest req)
        {
            // Note: session creation should be performed for the authenticated user.
            if (req == null)
                return BadRequest(new { error = "Request body is required." });

            // Extract userId from JWT claims to enforce security.
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userId = claim?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var session = await _chat.CreateSessionAsync(userId);
            return Ok(session);
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest req)
        {
            if (req == null)
                return BadRequest(new { error = "Request body is required." });

            // Extract userId from JWT claims (do not trust frontend-provided userId)
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userId = claim?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            //if (string.IsNullOrWhiteSpace(req.SessionId))
            //    return BadRequest(new { error = "sessionId is required." });

            if (string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { error = "message is required." });

            var aiMessage = await _chat.SendMessageAsync(userId, req.SessionId, req.Message);
            // aiMessage.SessionId will contain the session id used
            return Ok(new { sessionId = aiMessage.SessionId, reply = aiMessage.Content });
        }

        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userId = claim?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var history = await _chat.GetChatHistoryAsync(userId);
            // return empty array if no history
            return Ok(history ?? new object[0]);
        }
    }

    public class CreateSessionRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class SendMessageRequest
    {
        //public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }
}
using ChatApi.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ChatSessionsController : ControllerBase
{
    private readonly IChatSessionService _chatSessionService;
    
    public ChatSessionsController(IChatSessionService chatSessionService)
    {
        _chatSessionService = chatSessionService;
    }

    /// <summary>
    /// API Endpoint for starting a new chat session.
    /// POST: api/ChatSessions
    /// </summary>
    /// <returns>An action result representing the chat session creation status.</returns>
    [HttpPost]
    public async Task<IActionResult> StartChatSession()
    {
        try
        {
            var chatSession = await _chatSessionService.CreateSessionAsync();
            if (chatSession != null)
            {
                await _chatSessionService.AssignAgentToChatSessionAsync(); 
                return Ok(chatSession);
            }
            else
                return StatusCode(503, "Chat session could not be started due to server conditions.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// API Endpoint for polling a chat session.
    /// PUT: api/ChatSessions/{sessionId:int}/poll
    /// </summary>
    /// <param name="sessionId">The ID of the chat session to be polled.</param>
    /// <returns>An action result representing the poll status.</returns>
    [HttpPut("{sessionId:int}/poll")]
    public async Task<IActionResult> Poll(int sessionId)
    {
        try
        {
            var chatSession = await _chatSessionService.GetSessionAsync(sessionId);

            if (chatSession == null)
                return NotFound();

            chatSession.ReceivedPoll();
            await _chatSessionService.UpdateSessionAsync(chatSession);

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

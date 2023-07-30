using ChatApi.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Services.Abstraction
{
    public interface IChatSessionService
    {

        Task AssignAgentToChatSessionAsync();
        Task<ChatSession> GetSessionAsync(int sessionId);
        Task UpdateSessionAsync(ChatSession chatSession);
        Task<ChatSession?> CreateSessionAsync();
        Task<List<ChatSession>> GetActiveSessionsAsync();
        Task<List<ChatSession>> GetQueuedSessionsAsync();
    }
}

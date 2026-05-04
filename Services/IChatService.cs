using AIChatBot.Models;

namespace AIChatBot.Services
{
    public interface IChatService
    {
        Task<ChatSession> CreateSessionAsync(string userId);
        Task<Message> SendMessageAsync(string userId, string sessionId, string content);
        Task<IEnumerable<object>> GetHistoryAsync(string userId);
        Task<IEnumerable<object>> GetChatHistoryAsync(string userId);
    }
}
using AIChatBot.Models;

namespace AIChatBot.Repositories
{
    public interface IChatRepository
    {
        Task<ChatSession> CreateSessionAsync(ChatSession session);
        // Alias names requested in spec
        Task<ChatSession> CreateSession(ChatSession session);

        Task<IEnumerable<ChatSession>> GetSessionsByUserId(string userId);

        Task<Message> CreateMessageAsync(Message message);
        Task<Message> SaveMessage(Message message);

        Task<IEnumerable<Message>> GetMessagesBySessionId(string sessionId);
    }
}
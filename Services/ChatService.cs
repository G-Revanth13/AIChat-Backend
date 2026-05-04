using AIChatBot.Models;
using AIChatBot.Repositories;

namespace AIChatBot.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        private readonly IAIService _ai;

        public ChatService(IChatRepository repo, IAIService ai)
        {
            _repo = repo;
            _ai = ai;
        }

        public async Task<ChatSession> CreateSessionAsync(string userId)
        {
            var session = new ChatSession { UserId = userId, CreatedAt = DateTime.UtcNow };
            return await _repo.CreateSessionAsync(session);
        }

        public async Task<Message> SendMessageAsync(string userId, string sessionId, string content)
        {
            // If no sessionId supplied, create a new session for the user
            string usedSessionId = sessionId;
            if (string.IsNullOrWhiteSpace(usedSessionId))
            {
                var newSession = await CreateSessionAsync(userId);
                usedSessionId = newSession.Id;
            }

            // Save user message
            var userMessage = new Message
            {
                SessionId = usedSessionId,
                UserId = userId,
                Content = content,
                Role = "User",
                Timestamp = DateTime.UtcNow
            };

            await _repo.CreateMessageAsync(userMessage);

            // Call AI
            var aiResponse = await _ai.GetAIResponseAsync(content);

            var aiMessage = new Message
            {
                SessionId = usedSessionId,
                UserId = userId, // system/AI
                Content = aiResponse,
                Role = "AI",
                Timestamp = DateTime.UtcNow
            };

            await _repo.CreateMessageAsync(aiMessage);

            return aiMessage;
        }

        public async Task<IEnumerable<object>> GetHistoryAsync(string userId)
        {
            // legacy alias to GetChatHistoryAsync
            return await GetChatHistoryAsync(userId);
        }

        public async Task<IEnumerable<object>> GetChatHistoryAsync(string userId)
        {
            var sessions = await _repo.GetSessionsByUserId(userId);
            var result = new List<object>();

            foreach (var s in sessions)
            {
                var messages = await _repo.GetMessagesBySessionId(s.Id);
                // ensure messages are ordered ascending by timestamp
                var ordered = messages.OrderBy(m => m.Timestamp).Select(m => new { role = m.Role.ToLower(), content = m.Content });
                result.Add(new { sessionId = s.Id, messages = ordered });
            }

            return result;
        }
    }
}
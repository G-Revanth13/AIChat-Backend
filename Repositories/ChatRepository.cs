using AIChatBot.Data;
using AIChatBot.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AIChatBot.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly IMongoCollection<ChatSession> _sessions;
        private readonly IMongoCollection<Message> _messages;

        public ChatRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.DatabaseName);
            _sessions = db.GetCollection<ChatSession>("ChatSessions");
            _messages = db.GetCollection<Message>("Messages");
        }

        public async Task<ChatSession> CreateSessionAsync(ChatSession session)
        {
            await _sessions.InsertOneAsync(session);
            return session;
        }

        public async Task<ChatSession> CreateSession(ChatSession session)
        {
            return await CreateSessionAsync(session);
        }

        public async Task<IEnumerable<ChatSession>> GetSessionsByUserId(string userId)
        {
            return await _sessions.Find(s => s.UserId == userId).ToListAsync();
        }

        public async Task<Message> CreateMessageAsync(Message message)
        {
            await _messages.InsertOneAsync(message);
            return message;
        }

        public async Task<Message> SaveMessage(Message message)
        {
            return await CreateMessageAsync(message);
        }

        public async Task<IEnumerable<Message>> GetMessagesBySessionId(string sessionId)
        {
            return await _messages.Find(m => m.SessionId == sessionId).SortBy(m => m.Timestamp).ToListAsync();
        }
    }
}
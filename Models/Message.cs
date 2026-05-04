using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AIChatBot.Models
{
    public class Message
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string SessionId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public string Content { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
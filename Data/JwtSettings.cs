namespace AIChatBot.Data
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int ExpiryMinutes { get; set; }
        public string Issuer { get; set; } = "AIChatBot";
    }
}
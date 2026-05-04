namespace AIChatBot.Services
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string email);
    }
}
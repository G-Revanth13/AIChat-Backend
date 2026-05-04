namespace AIChatBot.Services
{
    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string message);
    }
}
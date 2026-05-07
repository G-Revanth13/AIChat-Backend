using AIChatBot.DTOs;
using AIChatBot.Models;

namespace AIChatBot.Services
{
    public interface IUserService
    {
        Task<AuthResult> LoginAsync(LoginModel model);
        Task<User> RegisterAsync(RegisterModel model);
    }

    public class AuthResult
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; }
    }
}
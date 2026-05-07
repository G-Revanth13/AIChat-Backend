using AIChatBot.DTOs;
using AIChatBot.Models;
using AIChatBot.Repositories;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AIChatBot.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IJwtService _jwt;

        public UserService(IUserRepository repo, IJwtService jwt)
        {
            _repo = repo;
            _jwt = jwt;
        }

        public async Task<AuthResult> LoginAsync(LoginModel model)
        {
            var user = await _repo.GetByEmailAsync(model.Email);
            if (user == null)
            {
                throw new ApplicationException("User Not Found");
            }

            if (!BCryptNet.Verify(model.Password, user.PasswordHash))
            {
                throw new ApplicationException("Invalid credentials");
            }

            var token = _jwt.GenerateToken(user.Id, user.Email);
            return new AuthResult { Token = token, UserId = user.Id ,Username=user.Username};
        }

        public async Task<User> RegisterAsync(RegisterModel model)
        {
            var existing = await _repo.GetByEmailAsync(model.Email);
            if (existing != null)
            {
                throw new ApplicationException("User already exists");
            }

            User user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCryptNet.HashPassword(model.Password)
            };

            await _repo.CreateAsync(user);
            return user;
        }
    }
}
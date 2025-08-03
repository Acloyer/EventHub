using EventHub.Models.DTOs;
<<<<<<< HEAD
using EventHub.Models;
=======
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

namespace EventHub.Services
{
    public interface IUserService
    {
        Task<AuthResponse> Register(RegisterDto model);
        Task<AuthResponse> Login(LoginDto model);
        Task<bool> UserExists(string email);
        Task<bool> DeleteUserAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task ValidateAndResetTelegramVerification(User user);
    }
} 
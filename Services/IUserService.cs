using EventHub.Models.DTOs;

namespace EventHub.Services
{
    public interface IUserService
    {
        Task<AuthResponse> Register(RegisterDto model);
        Task<AuthResponse> Login(LoginDto model);
        Task<bool> UserExists(string email);
        Task<bool> DeleteUserAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
} 
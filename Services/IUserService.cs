using EventHub.Models;
using EventHub.Models.DTOs;
using System.Threading.Tasks;

namespace EventHub.Services
{
    public interface IUserService
    {
        Task<UserDto?> Register(UserRegisterDto registerDto);
        Task<UserDto?> Login(UserLoginDto loginDto);
        Task<AuthResponse?> AuthenticateAsync(string email, string password);
        Task<AuthResponse?> RegisterAsync(string email, string password);
        Task<bool> AssignRoleAsync(int userId, string roleName);
        Task<bool> RemoveRoleAsync(int userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<bool> IsInRoleAsync(int userId, string roleName);
        Task<bool> DeleteUserAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
} 
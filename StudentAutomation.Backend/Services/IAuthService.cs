using StudentAutomation.Backend.DTOs;

namespace StudentAutomation.Backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> GetUserProfileAsync(string userId);
        Task<bool> AssignRoleAsync(string userId, string role);
        Task<List<string>> GetUserRolesAsync(string userId);
    }
}

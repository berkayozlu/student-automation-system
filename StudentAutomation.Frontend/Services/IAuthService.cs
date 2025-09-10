using StudentAutomation.Frontend.Models;

namespace StudentAutomation.Frontend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> GetProfileAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetUserRoleAsync();
        Task LogoutAsync();
        Task<string?> GetTokenAsync();
        Task<UserDto?> GetCurrentUserAsync();
    }
}

using StudentAutomation.Backend.Models;

namespace StudentAutomation.Backend.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(ApplicationUser user);
        string? GetUserIdFromToken(string token);
        bool ValidateToken(string token);
    }
}

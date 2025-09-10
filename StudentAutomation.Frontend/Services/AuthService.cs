using Blazored.LocalStorage;
using StudentAutomation.Frontend.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StudentAutomation.Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IConfiguration _configuration;
        private const string TokenKey = "authToken";
        private const string UserKey = "currentUser";

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (result != null && result.Success && !string.IsNullOrEmpty(result.Token))
                {
                    await _localStorage.SetItemAsync(TokenKey, result.Token);
                    await _localStorage.SetItemAsync(UserKey, result.User);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                }

                return result ?? new AuthResponseDto { Success = false, Message = "Login failed" };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Login error: {ex.Message}" };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    return new AuthResponseDto { Success = false, Message = $"Registration failed: {response.StatusCode} - Response: {responseContent}" };
                }

                // Debug: Log the actual response content
                Console.WriteLine($"API Response: {responseContent}");

                // Check if response starts with HTML
                if (responseContent.TrimStart().StartsWith("<"))
                {
                    return new AuthResponseDto { Success = false, Message = $"API returned HTML instead of JSON: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}" };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (result != null && result.Success && !string.IsNullOrEmpty(result.Token))
                {
                    await _localStorage.SetItemAsync(TokenKey, result.Token);
                    await _localStorage.SetItemAsync(UserKey, result.User);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                }

                return result ?? new AuthResponseDto { Success = false, Message = "Registration failed" };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Registration error: {ex.Message}" };
            }
        }

        public async Task<AuthResponseDto> GetProfileAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new AuthResponseDto { Success = false, Message = "No authentication token found" };
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/auth/profile");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                    if (result != null && result.Success && result.User != null)
                    {
                        await _localStorage.SetItemAsync(UserKey, result.User);
                    }
                    return result ?? new AuthResponseDto { Success = false, Message = "Profile retrieval failed" };
                }
                else
                {
                    await LogoutAsync();
                    return new AuthResponseDto { Success = false, Message = "Authentication expired" };
                }
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Profile error: {ex.Message}" };
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        public async Task<string?> GetUserRoleAsync()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                var role = user?.Roles?.FirstOrDefault();
                Console.WriteLine($"DEBUG: User roles: {string.Join(", ", user?.Roles ?? new List<string>())}, First role: {role}");
                return role;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error getting user role: {ex.Message}");
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(UserKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>(TokenKey);
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            return await _localStorage.GetItemAsync<UserDto>(UserKey);
        }
    }
}

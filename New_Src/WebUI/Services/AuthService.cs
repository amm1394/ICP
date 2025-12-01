using System.Text.Json;

namespace WebUI.Services;

// مدل‌های DTO برای تبادل داده
public record LoginRequest(string Email, string Password);
public record AuthResult(bool IsAuthenticated, string Message, string Name = "User", string Token = "");

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IHttpClientFactory clientFactory, ILogger<AuthService> logger)
    {
        // دریافت کلاینت تنظیم شده با آدرس API
        _httpClient = clientFactory.CreateClient("Api");
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest(email, password);
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new AuthResult(false, "Invalid server response");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new AuthResult(false, "Invalid credentials");

            return new AuthResult(false, $"Server Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login API Error");
            return new AuthResult(false, "Connection failed. Is API running?");
        }
    }

    public AuthResult GuestLogin()
    {
        return new AuthResult(true, "Guest Login", "Guest");
    }
}
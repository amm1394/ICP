using System.Text.Json;

namespace WebUI.Services;

// DTOs
public record LoginRequest(string Email, string Password, bool RememberMe = false);
public record RegisterRequest(string Email, string Password, string? FullName, string Position);
public record AuthResult(bool IsAuthenticated, string Message, string Name = "User", string Token = "", string Position = "User");

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    // برای نگهداری وضعیت کاربر فعلی (در نسخه واقعی از Session/Cookie استفاده کن)
    private static AuthResult? _currentUser;

    public AuthService(IHttpClientFactory clientFactory, ILogger<AuthService> logger)
    {
        _httpClient = clientFactory.CreateClient("Api");
        _logger = logger;
    }

    /// <summary>
    /// ورود کاربر
    /// </summary>
    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false)
    {
        try
        {
            var request = new LoginRequest(email, password, rememberMe);
            var response = await _httpClient.PostAsJsonAsync("auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.IsAuthenticated)
                {
                    _currentUser = result;
                    return result;
                }

                return new AuthResult(false, "Invalid server response");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new AuthResult(false, "Invalid email or password");

            return new AuthResult(false, $"Server Error: {response.StatusCode}");
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("API not available, using offline mode");
            // Offline mode - برای تست بدون API
            return OfflineLogin(email, password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login API Error");
            return new AuthResult(false, "Connection failed. Is API running? ");
        }
    }

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    public async Task<AuthResult> RegisterAsync(string email, string password, string? fullName, string position)
    {
        try
        {
            var request = new RegisterRequest(email, password, fullName, position);
            var response = await _httpClient.PostAsJsonAsync("auth/register", request);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResult(true, "Account created successfully");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return new AuthResult(false, "Email already registered");

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResult(false, $"Registration failed: {errorContent}");
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("API not available for registration");
            return new AuthResult(false, "Cannot register in offline mode.  Please ensure API is running.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register API Error");
            return new AuthResult(false, "Registration failed. Please try again.");
        }
    }

    /// <summary>
    /// بررسی آیا کاربر قبلاً Remember Me زده
    /// </summary>
    public async Task<AuthResult> CheckAutoLoginAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("auth/check-session");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResult>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null && result.IsAuthenticated)
                {
                    _currentUser = result;
                    return result;
                }
            }

            return new AuthResult(false, "No active session");
        }
        catch
        {
            return new AuthResult(false, "No active session");
        }
    }

    /// <summary>
    /// ورود مهمان
    /// </summary>
    public AuthResult GuestLogin()
    {
        _currentUser = new AuthResult(true, "Guest Login", "Guest", "", "Guest");
        return _currentUser;
    }

    /// <summary>
    /// خروج کاربر
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("auth/logout", null);
        }
        catch { }
        finally
        {
            _currentUser = null;
        }
    }

    /// <summary>
    /// دریافت کاربر فعلی
    /// </summary>
    public AuthResult? GetCurrentUser() => _currentUser;

    /// <summary>
    /// حالت آفلاین برای تست
    /// </summary>
    private AuthResult OfflineLogin(string email, string password)
    {
        // برای تست بدون API - هر ایمیل/پسورد قبول میشه
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            var name = email.Split('@')[0];
            name = char.ToUpper(name[0]) + name.Substring(1);

            _currentUser = new AuthResult(true, "Offline Login", name, "", "Analyst");
            return _currentUser;
        }

        return new AuthResult(false, "Invalid credentials");
    }
}
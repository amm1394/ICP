using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Wrapper;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IsatisDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IsatisDbContext db,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == request.Username.ToLower() ||
                    u.Email.ToLower() == request.Username.ToLower());

            if (user == null)
                return new AuthResult(false, Error: "Invalid username or password");

            if (!user.IsActive)
                return new AuthResult(false, Error: "User account is disabled");

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return new AuthResult(false, Error: "Invalid username or password");

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());
            user.LastLoginAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            return new AuthResult(
                true,
                accessToken,
                refreshToken,
                DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                MapToUserDto(user)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user {Username}", request.Username);
            return new AuthResult(false, Error: "Login failed");
        }
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (await _db.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
                return new AuthResult(false, Error: "Username already exists");

            if (await _db.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
                return new AuthResult(false, Error: "Email already exists");

            if (!UserRoles.All.Contains(request.Role))
                return new AuthResult(false, Error: $"Invalid role.  Valid roles: {string.Join(", ", UserRoles.All)}");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully", user.Username);

            return new AuthResult(
                true,
                accessToken,
                refreshToken,
                DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                MapToUserDto(user)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user {Username}", request.Username);
            return new AuthResult(false, Error: "Registration failed");
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                return new AuthResult(false, Error: "Invalid access token");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return new AuthResult(false, Error: "Invalid token claims");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return new AuthResult(false, Error: "User not found");

            if (user.RefreshToken != request.RefreshToken)
                return new AuthResult(false, Error: "Invalid refresh token");

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResult(false, Error: "Refresh token expired");

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());
            await _db.SaveChangesAsync();

            return new AuthResult(
                true,
                newAccessToken,
                newRefreshToken,
                DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                MapToUserDto(user)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return new AuthResult(false, Error: "Token refresh failed");
        }
    }

    public async Task<Result<bool>> LogoutAsync(Guid userId)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Result<bool>.Fail("User not found");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed for user {UserId}", userId);
            return Result<bool>.Fail("Logout failed");
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Result<bool>.Fail("User not found");

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return Result<bool>.Fail("Current password is incorrect");

            user.PasswordHash = HashPassword(request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Password changed for user {UserId}", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password failed for user {UserId}", userId);
            return Result<bool>.Fail("Change password failed");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Result<UserDto>.Fail("User not found");

            return Result<UserDto>.Success(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get user failed for {UserId}", userId);
            return Result<UserDto>.Fail("Failed to get user");
        }
    }

    public async Task<Result<List<UserListItemDto>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _db.Users
                .Select(u => new UserListItemDto(
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.LastLoginAt
                ))
                .ToListAsync();

            return Result<List<UserListItemDto>>.Success(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get all users failed");
            return Result<List<UserListItemDto>>.Fail("Failed to get users");
        }
    }

    public async Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Result<UserDto>.Fail("User not found");

            if (request.FirstName != null)
                user.FirstName = request.FirstName;
            if (request.LastName != null)
                user.LastName = request.LastName;
            if (request.Email != null)
            {
                if (await _db.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.UserId != userId))
                    return Result<UserDto>.Fail("Email already in use");
                user.Email = request.Email;
            }
            if (request.Role != null)
            {
                if (!UserRoles.All.Contains(request.Role))
                    return Result<UserDto>.Fail($"Invalid role. Valid roles: {string.Join(", ", UserRoles.All)}");
                user.Role = request.Role;
            }
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated", userId);
            return Result<UserDto>.Success(MapToUserDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update user failed for {UserId}", userId);
            return Result<UserDto>.Fail("Failed to update user");
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Result<bool>.Fail("User not found");

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete user failed for {UserId}", userId);
            return Result<bool>.Fail("Failed to delete user");
        }
    }

    public async Task<Guid?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
                return user?.UserId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #region Private Methods - JWT

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user. UserId.ToString()),
            new Claim(ClaimTypes. Name, user.Username),
            new Claim(ClaimTypes. Email, user.Email),
            new Claim(ClaimTypes.Role, user. Role),
            new Claim("firstName", user.FirstName ??  ""),
            new Claim("lastName", user.LastName ??  "")
        };

        var token = new JwtSecurityToken(
            issuer: GetJwtIssuer(),
            audience: GetJwtAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(GetJwtSecret());

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateLifetime = false
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Private Methods - Password

    private string HashPassword(string password)
    {
        var salt = GenerateSalt();
        using var sha256 = SHA256.Create();
        var saltedPassword = salt + password;
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return salt + ":" + Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = parts[0];
        var hash = parts[1];

        using var sha256 = SHA256.Create();
        var saltedPassword = salt + password;
        var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword)));

        return hash == computedHash;
    }

    private string GenerateSalt()
    {
        var salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    #endregion

    #region Private Methods - Configuration

    private string GetJwtSecret() =>
        _configuration["Jwt:Secret"] ?? "IsatisICP-SuperSecret-Key-2024-Must-Be-At-Least-32-Characters! ";

    private string GetJwtIssuer() =>
        _configuration["Jwt:Issuer"] ?? "IsatisICP";

    private string GetJwtAudience() =>
        _configuration["Jwt:Audience"] ?? "IsatisICP-Users";

    private int GetAccessTokenExpiryMinutes() =>
        int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var minutes) ? minutes : 60;

    private int GetRefreshTokenExpiryDays() =>
        int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 7;

    #endregion

    #region Private Methods - Mapping

    private static UserDto MapToUserDto(User user) => new(
        user.UserId,
        user.Username,
        user.Email,
        user.FirstName,
        user.LastName,
        user.Role,
        user.IsActive,
        user.CreatedAt,
        user.LastLoginAt
    );

    #endregion
}
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

// ============================================
// Request DTOs
// ============================================

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

public record RegisterRequest(
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    string Username,

    [Required]
    [EmailAddress]
    string Email,

    [Required]
    [MinLength(6)]
    string Password,

    string? FirstName = null,
    string? LastName = null,
    string Role = "Viewer"
);

public record RefreshTokenRequest(
    [Required] string AccessToken,
    [Required] string RefreshToken
);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required]
    [MinLength(6)]
    string NewPassword
);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Role,
    bool? IsActive
);

// ============================================
// Response DTOs
// ============================================

public record AuthResult(
    bool Succeeded,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    UserDto? User = null,
    string? Error = null
);

public record UserDto(
    Guid UserId,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record UserListItemDto(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt
);
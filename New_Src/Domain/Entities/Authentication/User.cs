using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// User entity for authentication
/// </summary>
public class User
{
    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// User role: Admin, Analyst, Viewer
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = UserRoles.Viewer;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Refresh token for JWT refresh
    /// </summary>
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
}

/// <summary>
/// Available user roles
/// </summary>
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Analyst = "Analyst";
    public const string Viewer = "Viewer";

    public static readonly string[] All = { Admin, Analyst, Viewer };
}
using Application.DTOs;
using Shared.Wrapper;

namespace Application.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result<bool>> LogoutAsync(Guid userId);
    Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Result<List<UserListItemDto>>> GetAllUsersAsync();
    Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<Result<bool>> DeleteUserAsync(Guid userId);
    Task<Guid?> ValidateTokenAsync(string token);
}
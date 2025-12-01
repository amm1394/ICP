using System.Security.Claims;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Wrapper;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Succeeded)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.Succeeded)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> Logout()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(Result<bool>.Fail("Invalid token"));

        var result = await _authService.LogoutAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<Result<UserDto>>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(Result<UserDto>.Fail("Invalid token"));

        var result = await _authService.GetUserByIdAsync(userId.Value);
        if (!result.Succeeded)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(Result<bool>.Fail("Invalid token"));

        var result = await _authService.ChangePasswordAsync(userId.Value, request);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("users")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<Result<List<UserListItemDto>>>> GetAllUsers()
    {
        var result = await _authService.GetAllUsersAsync();
        return Ok(result);
    }

    [HttpGet("users/{userId:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<Result<UserDto>>> GetUser(Guid userId)
    {
        var result = await _authService.GetUserByIdAsync(userId);
        if (!result.Succeeded)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPut("users/{userId:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<Result<UserDto>>> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var result = await _authService.UpdateUserAsync(userId, request);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("users/{userId:guid}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<Result<bool>>> DeleteUser(Guid userId)
    {
        var result = await _authService.DeleteUserAsync(userId);
        if (!result.Succeeded)
            return BadRequest(result);
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
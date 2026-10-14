using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRHub.Application.Common.Models;
using QRHub.Application.DTOs.Auth;
using QRHub.Application.Services;

namespace QRHub.Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user with name, email, and password
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponse>.Ok(response, "User registered successfully."));
    }

    /// <summary>
    /// Login with registered email and password to receive a JWT Bearer token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }

    /// <summary>
    /// Retrieve current authenticated user profile
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await _authService.GetUserProfileAsync(CurrentUserId, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(profile, "Profile retrieved successfully."));
    }
}

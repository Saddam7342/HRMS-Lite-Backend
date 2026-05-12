using Asp.Versioning;
using HRMS.Application.Features.Auth.Commands.Account;
using HRMS.Application.Features.Auth.Commands.Login;
using HRMS.Application.Features.Auth.Commands.Refresh;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Application.Features.Auth.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class AuthController : BaseApiController
{
    /// <summary>
    /// Authenticates a user and returns a JWT token pair.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : UnauthorizedData(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkData(result) : UnauthorizedData(result);
    }

    /// <summary>
    /// Revokes the given refresh token (idempotent — unknown tokens still succeed).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? OkEmpty(result) : BadEmpty(result);
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return result.IsSuccess ? OkData(result) : UnauthorizedData(result);
    }
}

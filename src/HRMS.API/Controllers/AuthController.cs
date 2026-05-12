using HRMS.Application.Features.Auth.Commands.Account;
using HRMS.Application.Features.Auth.Commands.Login;
using HRMS.Application.Features.Auth.Commands.Refresh;
using HRMS.Application.Features.Auth.DTOs;
using HRMS.Application.Features.Auth.Queries;
using HRMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace HRMS.API.Controllers;

[ApiVersion("1.0")]
public class AuthController : BaseApiController
{
    /// <summary>
    /// Authenticates a user and returns a JWT token pair.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess 
            ? Ok(ApiResponse<LoginResponse>.Ok(result.Data!)) 
            : BadRequest(ApiResponse<LoginResponse>.Fail(result.Errors));
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), 200)]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess 
            ? Ok(ApiResponse<TokenDto>.Ok(result.Data!)) 
            : BadRequest(ApiResponse<TokenDto>.Fail(result.Errors));
    }

    /// <summary>
    /// Revokes the current refresh token and logs out the user.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(LogoutCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(ApiResponse.Ok()) : BadRequest(ApiResponse.Fail(result.Errors));
    }

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), 200)]
    public async Task<IActionResult> GetMe()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return result.IsSuccess 
            ? Ok(ApiResponse<CurrentUserDto>.Ok(result.Data!)) 
            : Unauthorized(ApiResponse<CurrentUserDto>.Fail(result.Errors));
    }
}

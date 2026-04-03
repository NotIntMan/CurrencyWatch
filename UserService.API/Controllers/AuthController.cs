using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using UserService.API.DTOs;
using UserService.Application.Commands.Login;
using UserService.Application.Commands.Logout;
using UserService.Application.Commands.Refresh;
using UserService.Application.Commands.RegisterUser;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti)!;
        var exp = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Exp)!);
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;

        var command = new LogoutCommand(jti, expiresAt, request.RefreshToken);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

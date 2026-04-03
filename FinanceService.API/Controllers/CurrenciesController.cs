using System.Security.Claims;
using FinanceService.Application.Commands.AddFavorite;
using FinanceService.Application.Commands.RemoveFavorite;
using FinanceService.Application.Queries.GetAllCurrencies;
using FinanceService.Application.Queries.GetUserCurrencies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinanceService.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    private int UserId => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    public CurrenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCurrenciesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserCurrenciesQuery(UserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("favorites/{charCode}")]
    public async Task<IActionResult> AddFavorite(string charCode, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AddFavoriteCommand(UserId, charCode), cancellationToken);
        return NoContent();
    }

    [HttpDelete("favorites/{charCode}")]
    public async Task<IActionResult> RemoveFavorite(string charCode, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveFavoriteCommand(UserId, charCode), cancellationToken);
        return NoContent();
    }
}

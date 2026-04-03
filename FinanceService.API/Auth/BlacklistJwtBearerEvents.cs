using System.Security.Claims;
using Common.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinanceService.API.Auth;

public class BlacklistJwtBearerEvents : JwtBearerEvents
{
    public override async Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = context.AuthenticateFailure?.Message,
        };

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    public override async Task Forbidden(ForbiddenContext context)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
        };

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (jti is null)
        {
            context.Fail("Token has no jti claim.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        if (await db.TokenBlacklist.AnyAsync(t => t.Jti == jti))
        {
            context.Fail("Token has been revoked.");
        }
    }
}

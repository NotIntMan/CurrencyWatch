using System.Security.Claims;
using Common.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace UserService.API.Auth;

public class BlacklistJwtBearerEvents : JwtBearerEvents
{
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

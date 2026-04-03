using Common.API.Auth;
using Common.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtBearerWithBlacklist(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<BlacklistJwtBearerEvents>();

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()!;
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = jwtOptions.ToValidationParameters();
                options.EventsType = typeof(BlacklistJwtBearerEvents);
            });

        return services;
    }
}

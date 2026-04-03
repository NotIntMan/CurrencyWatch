using Common.API.Auth;
using Common.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

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

    public static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    []
                },
            });
        });

        return services;
    }
}

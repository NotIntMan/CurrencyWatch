using Common.Database;
using Common.Infrastructure;
using Common.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using UserService.API.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddSingleton<PasswordHasher>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtService>();

builder.Services.AddScoped<BlacklistJwtBearerEvents>();
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = jwtOptions.ToValidationParameters();
        options.EventsType = typeof(BlacklistJwtBearerEvents);
    });

builder.Services.AddMediatR(
    cfg => cfg.RegisterServicesFromAssemblyContaining<UserService.Application.Commands.Login.LoginCommand>()
);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

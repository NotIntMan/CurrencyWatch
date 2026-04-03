using Common.Database;
using Common.Domain.Entities;
using UserService.Application.Services;
using Common.Infrastructure.Jwt;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;
using UserService.Application.Exceptions;

namespace UserService.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtService _jwtService;

    public LoginCommandHandler(AppDbContext db, PasswordHasher passwordHasher, JwtService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == request.Name, ct);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid name or password.");
        }

        var accessToken = _jwtService.GenerateToken(user);

        var refreshToken = RefreshToken.Create(user.Id);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return new LoginResultDto(accessToken, refreshToken.Token);
    }
}

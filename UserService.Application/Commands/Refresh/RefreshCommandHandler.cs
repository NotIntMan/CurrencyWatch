using Common.Database;
using Common.Domain.Entities;
using Common.Infrastructure.Jwt;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application.DTOs;

namespace UserService.Application.Commands.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, LoginResultDto>
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwtService;

    public RefreshCommandHandler(AppDbContext db, JwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<LoginResultDto> Handle(RefreshCommand request, CancellationToken ct)
    {
        var oldToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, ct);

        if (oldToken is null || oldToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await _db.Users.FirstAsync(u => u.Id == oldToken.UserId, ct);

        var accessToken = _jwtService.GenerateToken(user);

        _db.RefreshTokens.Remove(oldToken);

        var newRefreshToken = RefreshToken.Create(user.Id);
        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync(ct);

        return new LoginResultDto(accessToken, newRefreshToken.Token);
    }
}

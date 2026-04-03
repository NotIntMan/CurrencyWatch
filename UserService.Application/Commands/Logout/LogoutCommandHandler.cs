using Common.Database;
using Common.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly AppDbContext _db;

    public LogoutCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task Handle(LogoutCommand request, CancellationToken ct)
    {
        _db.TokenBlacklist.Add(new TokenBlacklistEntry
        {
            Jti = request.Jti,
            ExpiresAt = request.AccessTokenExpiresAt,
        });

        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == request.RefreshToken, ct);
        if (refreshToken is not null)
        {
            _db.RefreshTokens.Remove(refreshToken);
        }

        await _db.SaveChangesAsync(ct);
    }
}

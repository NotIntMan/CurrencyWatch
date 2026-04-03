using Common.Database;
using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.Logout;
using UserService.Application.Tests.TestHelpers;

namespace UserService.Application.Tests.Commands.Logout;

public sealed class LogoutCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly LogoutCommandHandler _sut;

    public LogoutCommandHandlerTests()
    {
        _sut = new LogoutCommandHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_BlacklistsAndRemovesRefreshToken()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "refresh-abc",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });
        await _db.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        await _sut.Handle(new LogoutCommand("jti-123", expiresAt, "refresh-abc"), CancellationToken.None);

        (await _db.TokenBlacklist.AnyAsync(t => t.Jti == "jti-123")).Should().BeTrue();
        (await _db.RefreshTokens.AnyAsync(r => r.Token == "refresh-abc")).Should().BeFalse();
    }
}

using Common.Database;
using Common.Domain.Entities;
using Common.Infrastructure.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Application.Commands.Refresh;
using UserService.Application.Tests.TestHelpers;

namespace UserService.Application.Tests.Commands.Refresh;

public sealed class RefreshCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly RefreshCommandHandler _sut;

    public RefreshCommandHandlerTests()
    {
        var jwtService = new JwtService(Options.Create(new JwtOptions
        {
            SecretKey = "test-secret-key-that-is-long-enough-for-hmac256",
            Issuer = "test-issuer",
            Audience = "test-audience",
        }));
        _sut = new RefreshCommandHandler(_db, jwtService);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_ValidToken()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "old-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new RefreshCommand("old-refresh"), CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe("old-refresh");
        (await _db.RefreshTokens.AnyAsync(r => r.Token == "old-refresh")).Should().BeFalse();
        (await _db.RefreshTokens.AnyAsync(r => r.Token == result.RefreshToken)).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidToken()
    {
        var act = () => _sut.Handle(new RefreshCommand("nonexistent"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired refresh token*");
    }

    [Fact]
    public async Task Handle_ExpiredToken()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "expired-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
        });
        await _db.SaveChangesAsync();

        var act = () => _sut.Handle(new RefreshCommand("expired-refresh"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired refresh token*");
    }
}

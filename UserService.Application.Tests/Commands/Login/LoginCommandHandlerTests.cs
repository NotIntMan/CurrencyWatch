using Common.Database;
using Common.Domain.Entities;
using Common.Infrastructure;
using Common.Infrastructure.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Application.Commands.Login;
using UserService.Application.Exceptions;
using UserService.Application.Tests.TestHelpers;

namespace UserService.Application.Tests.Commands.Login;

public sealed class LoginCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly PasswordHasher _passwordHasher = new();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        var jwtService = new JwtService(Options.Create(new JwtOptions
        {
            SecretKey = "test-secret-key-that-is-long-enough-for-hmac256",
            Issuer = "test-issuer",
            Audience = "test-audience",
        }));
        _sut = new LoginCommandHandler(_db, _passwordHasher, jwtService);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_ValidCredentials()
    {
        _db.Users.Add(new User { Name = "alice", PasswordHash = _passwordHasher.Hash("pass123") });
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new LoginCommand("alice", "pass123"), CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        (await _db.RefreshTokens.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WrongPassword()
    {
        _db.Users.Add(new User { Name = "alice", PasswordHash = _passwordHasher.Hash("pass123") });
        await _db.SaveChangesAsync();

        var act = () => _sut.Handle(new LoginCommand("alice", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("*Invalid name or password*");
        (await _db.RefreshTokens.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_NonexistentUser()
    {
        var act = () => _sut.Handle(new LoginCommand("nobody", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>()
            .WithMessage("*Invalid name or password*");
        (await _db.RefreshTokens.CountAsync()).Should().Be(0);
    }
}

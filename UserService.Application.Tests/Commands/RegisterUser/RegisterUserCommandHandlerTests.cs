using Common.Database;
using Common.Domain.Entities;
using UserService.Application.Services;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Commands.RegisterUser;
using UserService.Application.DTOs;
using UserService.Application.Exceptions;
using UserService.Application.Tests.TestHelpers;

namespace UserService.Application.Tests.Commands.RegisterUser;

public sealed class RegisterUserCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _sut = new RegisterUserCommandHandler(_db, new PasswordHasher());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_ValidRequest()
    {
        var result = await _sut.Handle(new RegisterUserCommand("alice", "pass123"), CancellationToken.None);

        result.Should().BeEquivalentTo(new UserDto(result.Id, "alice"));
        result.Id.Should().BeGreaterThan(0);

        var user = await _db.Users.SingleAsync(u => u.Name == "alice");
        user.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_DuplicateName()
    {
        _db.Users.Add(new User { Name = "alice", PasswordHash = "hash" });
        await _db.SaveChangesAsync();

        var act = () => _sut.Handle(new RegisterUserCommand("alice", "pass123"), CancellationToken.None);

        await act.Should().ThrowAsync<UserAlreadyExistsException>();

        (await _db.Users.CountAsync(u => u.Name == "alice")).Should().Be(1);
    }
}

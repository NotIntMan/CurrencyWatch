using Common.Database;
using Common.Domain.Entities;
using FinanceService.Application.Commands.AddFavorite;
using FinanceService.Application.Exceptions;
using FinanceService.Application.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Application.Tests.Commands.AddFavorite;

public sealed class AddFavoriteCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly AddFavoriteCommandHandler _sut;

    public AddFavoriteCommandHandlerTests()
    {
        _sut = new AddFavoriteCommandHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_AddsFavorite()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        _db.Users.Add(user);
        _db.Currencies.Add(usd);
        await _db.SaveChangesAsync();

        await _sut.Handle(new AddFavoriteCommand(user.Id, "USD"), CancellationToken.None);

        (await _db.UserFavorites.AnyAsync(f => f.UserId == user.Id && f.CurrencyId == usd.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateFavorite()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        _db.Users.Add(user);
        _db.Currencies.Add(usd);
        await _db.SaveChangesAsync();

        _db.UserFavorites.Add(new UserFavoriteCurrency { UserId = user.Id, CurrencyId = usd.Id });
        await _db.SaveChangesAsync();

        var act = () => _sut.Handle(new AddFavoriteCommand(user.Id, "USD"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        (await _db.UserFavorites.CountAsync(f => f.UserId == user.Id && f.CurrencyId == usd.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Handle_AddsOnlyForRequestedUser()
    {
        var alice = new User { Name = "alice", PasswordHash = "hash" };
        var bob = new User { Name = "bob", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        _db.Users.AddRange(alice, bob);
        _db.Currencies.Add(usd);
        await _db.SaveChangesAsync();

        await _sut.Handle(new AddFavoriteCommand(alice.Id, "USD"), CancellationToken.None);

        (await _db.UserFavorites.AnyAsync(f => f.UserId == alice.Id && f.CurrencyId == usd.Id)).Should().BeTrue();
        (await _db.UserFavorites.AnyAsync(f => f.UserId == bob.Id && f.CurrencyId == usd.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UnknownCharCode()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var act = () => _sut.Handle(new AddFavoriteCommand(user.Id, "XXX"), CancellationToken.None);

        await act.Should().ThrowAsync<CurrencyNotFoundException>();
    }
}

using Common.Database;
using Common.Domain.Entities;
using FinanceService.Application.Commands.RemoveFavorite;
using FinanceService.Application.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Application.Tests.Commands.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly RemoveFavoriteCommandHandler _sut;

    public RemoveFavoriteCommandHandlerTests()
    {
        _sut = new RemoveFavoriteCommandHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_RemovesFavorite()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        _db.Users.Add(user);
        _db.Currencies.Add(usd);
        await _db.SaveChangesAsync();

        _db.UserFavorites.Add(new UserFavoriteCurrency { UserId = user.Id, CurrencyId = usd.Id });
        await _db.SaveChangesAsync();

        await _sut.Handle(new RemoveFavoriteCommand(user.Id, "USD"), CancellationToken.None);

        (await _db.UserFavorites.AnyAsync(f => f.UserId == user.Id && f.CurrencyId == usd.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RemovesOnlyRequestedUserFavorite()
    {
        var alice = new User { Name = "alice", PasswordHash = "hash" };
        var bob = new User { Name = "bob", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        _db.Users.AddRange(alice, bob);
        _db.Currencies.Add(usd);
        await _db.SaveChangesAsync();

        _db.UserFavorites.AddRange(
            new UserFavoriteCurrency { UserId = alice.Id, CurrencyId = usd.Id },
            new UserFavoriteCurrency { UserId = bob.Id, CurrencyId = usd.Id }
        );
        await _db.SaveChangesAsync();

        await _sut.Handle(new RemoveFavoriteCommand(alice.Id, "USD"), CancellationToken.None);

        (await _db.UserFavorites.AnyAsync(f => f.UserId == alice.Id && f.CurrencyId == usd.Id)).Should().BeFalse();
        (await _db.UserFavorites.AnyAsync(f => f.UserId == bob.Id && f.CurrencyId == usd.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistentFavorite()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        _db.Users.Add(user);
        _db.Currencies.Add(usd);
        await _db.SaveChangesAsync();

        var act = () => _sut.Handle(new RemoveFavoriteCommand(user.Id, "USD"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

using Common.Database;
using Common.Domain.Entities;
using FinanceService.Application.DTOs;
using FinanceService.Application.Queries.GetUserCurrencies;
using FinanceService.Application.Tests.TestHelpers;

namespace FinanceService.Application.Tests.Queries.GetUserCurrencies;

public sealed class GetUserCurrenciesQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly GetUserCurrenciesQueryHandler _sut;

    public GetUserCurrenciesQueryHandlerTests()
    {
        _sut = new GetUserCurrenciesQueryHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_WithFavorites()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        var eur = new Currency { CharCode = "EUR", Name = "Евро", Rate = 100m };
        _db.Users.Add(user);
        _db.Currencies.AddRange(usd, eur);
        await _db.SaveChangesAsync();

        _db.UserFavorites.Add(new UserFavoriteCurrency { UserId = user.Id, CurrencyId = usd.Id });
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new GetUserCurrenciesQuery(user.Id), CancellationToken.None);

        result.Should().BeEquivalentTo(new[] { new CurrencyDto("USD", "Доллар США", 90m) });
    }

    [Fact]
    public async Task Handle_ReturnsOnlyRequestedUserFavorites()
    {
        var alice = new User { Name = "alice", PasswordHash = "hash" };
        var bob = new User { Name = "bob", PasswordHash = "hash" };
        var usd = new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m };
        var eur = new Currency { CharCode = "EUR", Name = "Евро", Rate = 100m };
        _db.Users.AddRange(alice, bob);
        _db.Currencies.AddRange(usd, eur);
        await _db.SaveChangesAsync();

        _db.UserFavorites.Add(new UserFavoriteCurrency { UserId = alice.Id, CurrencyId = usd.Id });
        _db.UserFavorites.Add(new UserFavoriteCurrency { UserId = bob.Id, CurrencyId = eur.Id });
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new GetUserCurrenciesQuery(alice.Id), CancellationToken.None);

        result.Should().BeEquivalentTo(new[] { new CurrencyDto("USD", "Доллар США", 90m) });
    }

    [Fact]
    public async Task Handle_NoFavorites()
    {
        var user = new User { Name = "alice", PasswordHash = "hash" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new GetUserCurrenciesQuery(user.Id), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

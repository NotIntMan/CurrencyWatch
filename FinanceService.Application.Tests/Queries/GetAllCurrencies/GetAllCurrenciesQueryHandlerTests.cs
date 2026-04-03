using Common.Database;
using Common.Domain.Entities;
using FinanceService.Application.DTOs;
using FinanceService.Application.Queries.GetAllCurrencies;
using FinanceService.Application.Tests.TestHelpers;

namespace FinanceService.Application.Tests.Queries.GetAllCurrencies;

public sealed class GetAllCurrenciesQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _db = TestDbContext.Create();
    private readonly GetAllCurrenciesQueryHandler _sut;

    public GetAllCurrenciesQueryHandlerTests()
    {
        _sut = new GetAllCurrenciesQueryHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Handle_WithCurrencies()
    {
        _db.Currencies.AddRange(
            new Currency { CharCode = "USD", Name = "Доллар США", Rate = 90m },
            new Currency { CharCode = "EUR", Name = "Евро", Rate = 100m }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.Handle(new GetAllCurrenciesQuery(), CancellationToken.None);

        result.Should().BeEquivalentTo(new[]
        {
            new CurrencyDto("USD", "Доллар США", 90m),
            new CurrencyDto("EUR", "Евро", 100m),
        });
    }

    [Fact]
    public async Task Handle_EmptyTable()
    {
        var result = await _sut.Handle(new GetAllCurrenciesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}

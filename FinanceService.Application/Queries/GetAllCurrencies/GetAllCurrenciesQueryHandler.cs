using Common.Database;
using FinanceService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Application.Queries.GetAllCurrencies;

public class GetAllCurrenciesQueryHandler : IRequestHandler<GetAllCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    private readonly AppDbContext _db;

    public GetAllCurrenciesQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CurrencyDto>> Handle(GetAllCurrenciesQuery request, CancellationToken ct)
    {
        return await _db.Currencies
            .Select(c => new CurrencyDto(c.CharCode, c.Name, c.Rate))
            .ToListAsync(ct);
    }
}

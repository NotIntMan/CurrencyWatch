using Common.Database;
using FinanceService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Application.Queries.GetUserCurrencies;

public class GetUserCurrenciesQueryHandler : IRequestHandler<GetUserCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    private readonly AppDbContext _db;

    public GetUserCurrenciesQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CurrencyDto>> Handle(GetUserCurrenciesQuery request, CancellationToken ct)
    {
        var query = from fav in _db.UserFavorites
                    where fav.UserId == request.UserId
                    join currency in _db.Currencies on fav.CurrencyId equals currency.Id
                    select new CurrencyDto(currency.CharCode, currency.Name, currency.Rate);

        return await query.ToListAsync(ct);
    }
}

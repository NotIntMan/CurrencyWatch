using Common.Database;
using Common.Domain.Entities;
using FinanceService.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Application.Commands.AddFavorite;

public class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, Unit>
{
    private readonly AppDbContext _db;

    public AddFavoriteCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(AddFavoriteCommand request, CancellationToken ct)
    {
        var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.CharCode == request.CharCode, ct)
            ?? throw new CurrencyNotFoundException(request.CharCode);

        var alreadyExists = await _db.UserFavorites
            .AnyAsync(f => f.UserId == request.UserId && f.CurrencyId == currency.Id, ct);

        if (!alreadyExists)
        {
            _db.UserFavorites.Add(new UserFavoriteCurrency { UserId = request.UserId, CurrencyId = currency.Id });
        }

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

using Common.Database;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Application.Commands.RemoveFavorite;

public class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, Unit>
{
    private readonly AppDbContext _db;

    public RemoveFavoriteCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(RemoveFavoriteCommand request, CancellationToken ct)
    {
        var query = from fav in _db.UserFavorites
                    where fav.UserId == request.UserId
                    join currency in _db.Currencies on fav.CurrencyId equals currency.Id
                    where currency.CharCode == request.CharCode
                    select fav;

        var favorite = await query.FirstOrDefaultAsync(ct);

        if (favorite is not null)
        {
            _db.UserFavorites.Remove(favorite);
        }

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

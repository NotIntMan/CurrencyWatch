namespace FinanceService.Application.Commands.AddFavorite;

public record AddFavoriteCommand(int UserId, string CharCode) : IRequest<Unit>;

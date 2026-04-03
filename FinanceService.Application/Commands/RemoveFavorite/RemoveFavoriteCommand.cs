namespace FinanceService.Application.Commands.RemoveFavorite;

public record RemoveFavoriteCommand(int UserId, string CharCode) : IRequest<Unit>;

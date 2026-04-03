using FinanceService.Application.DTOs;

namespace FinanceService.Application.Queries.GetUserCurrencies;

public record GetUserCurrenciesQuery(int UserId) : IRequest<IReadOnlyList<CurrencyDto>>;

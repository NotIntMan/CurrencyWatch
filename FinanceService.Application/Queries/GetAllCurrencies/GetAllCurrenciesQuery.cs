using FinanceService.Application.DTOs;

namespace FinanceService.Application.Queries.GetAllCurrencies;

public record GetAllCurrenciesQuery : IRequest<IReadOnlyList<CurrencyDto>>;

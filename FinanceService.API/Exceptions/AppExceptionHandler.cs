using Common.API.Exceptions;
using FinanceService.Application.Exceptions;

namespace FinanceService.API.Exceptions;

public class AppExceptionHandler : AppExceptionHandlerBase
{
    protected override (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        CurrencyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
    };
}

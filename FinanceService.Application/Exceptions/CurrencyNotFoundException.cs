namespace FinanceService.Application.Exceptions;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException(string charCode)
        : base($"Currency '{charCode}' not found.")
    {
    }
}

using System.Globalization;
using System.Xml.Linq;
using Common.Database;
using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyUpdater.Worker.Commands;

public class UpdateCurrenciesCommandHandler
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");
    private const string CbrUrl = "https://www.cbr.ru/scripts/XML_daily.asp";

    private readonly HttpClient _httpClient;
    private readonly AppDbContext _db;
    private readonly ILogger<UpdateCurrenciesCommandHandler> _logger;

    public UpdateCurrenciesCommandHandler(
        HttpClient httpClient,
        AppDbContext db,
        ILogger<UpdateCurrenciesCommandHandler> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateCurrenciesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching currencies from CBR...");

        using var response = await _httpClient.GetAsync(CbrUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var parsed = ParseCurrencies(doc);
        await UpsertAsync(parsed, cancellationToken);

        _logger.LogInformation("Updated succeeded");
    }

    private static List<Currency> ParseCurrencies(XDocument doc)
    {
        var result = new List<Currency>();

        foreach (var valute in doc.Descendants("Valute"))
        {
            var charCode = valute.Element("CharCode")?.Value;
            var name = valute.Element("Name")?.Value;
            var valueStr = valute.Element("Value")?.Value;
            var nominalStr = valute.Element("Nominal")?.Value;

            if (charCode is null || name is null || valueStr is null || nominalStr is null)
                continue;

            var value = decimal.Parse(valueStr, RuCulture);
            var nominal = decimal.Parse(nominalStr, RuCulture);

            result.Add(new Currency
            {
                Name = name,
                CharCode = charCode,
                Rate = value / nominal,
            });
        }

        return result;
    }

    private async Task UpsertAsync(List<Currency> currencies, CancellationToken ct)
    {
        var existing = await _db.Currencies
            .ToDictionaryAsync(c => c.CharCode, ct);

        foreach (var currency in currencies)
        {
            if (existing.TryGetValue(currency.CharCode, out var found))
            {
                found.Rate = currency.Rate;
            }
            else
            {
                _db.Currencies.Add(currency);
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
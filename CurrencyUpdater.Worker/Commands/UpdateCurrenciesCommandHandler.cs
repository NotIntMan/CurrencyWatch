using System.Xml.Linq;
using Common.Database;
using Common.Domain.Entities;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CurrencyUpdater.Worker.Commands;

public class UpdateCurrenciesCommandHandler
{
    private const string CbrUrl = "https://www.cbr.ru/scripts/XML_daily.asp";

    private readonly HttpClient _httpClient;
    private readonly AppDbContext _db;
    private readonly CbrXmlParser _parser;
    private readonly ILogger<UpdateCurrenciesCommandHandler> _logger;

    public UpdateCurrenciesCommandHandler(
        HttpClient httpClient,
        AppDbContext db,
        CbrXmlParser parser,
        ILogger<UpdateCurrenciesCommandHandler> logger
    )
    {
        _httpClient = httpClient;
        _db = db;
        _parser = parser;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateCurrenciesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching currencies from CBR...");

        using var response = await _httpClient.GetAsync(CbrUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var parsed = _parser.Parse(doc);
        await UpsertAsync(parsed, cancellationToken);

        _logger.LogInformation("Update succeeded");
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
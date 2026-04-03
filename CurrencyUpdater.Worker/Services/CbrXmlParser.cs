using System.Globalization;
using System.Xml.Linq;
using Common.Domain.Entities;

namespace CurrencyUpdater.Worker.Services;

public class CbrXmlParser
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    private readonly ILogger<CbrXmlParser> _logger;

    public CbrXmlParser(ILogger<CbrXmlParser> logger)
    {
        _logger = logger;
    }

    public List<Currency> Parse(XDocument doc)
    {
        var result = new List<Currency>();

        foreach (var valute in doc.Descendants("Valute"))
        {
            var currency = ParseValute(valute);
            if (currency is not null)
            {
                result.Add(currency);
            }
        }

        return result;
    }

    private Currency? ParseValute(XElement valute)
    {
        var charCode = valute.Element("CharCode")?.Value;
        var name = valute.Element("Name")?.Value;
        var valueStr = valute.Element("Value")?.Value;
        var nominalStr = valute.Element("Nominal")?.Value;

        if (charCode is null || name is null || valueStr is null || nominalStr is null)
        {
            _logger.LogWarning("Skipping Valute element with missing fields: {Xml}", valute);
            return null;
        }

        if (!decimal.TryParse(valueStr, RuCulture, out var value) ||
            !decimal.TryParse(nominalStr, RuCulture, out var nominal))
        {
            _logger.LogWarning("Failed to parse Value/Nominal for {CharCode}: Value={Value}, Nominal={Nominal}",
                charCode, valueStr, nominalStr);
            return null;
        }

        return new Currency
        {
            Name = name,
            CharCode = charCode,
            Rate = value / nominal,
        };
    }
}

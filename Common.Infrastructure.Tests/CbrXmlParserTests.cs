using System.Xml.Linq;
using Common.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Common.Infrastructure.Tests;

public class CbrXmlParserTests
{
    private readonly FakeLogger<CbrXmlParser> _logger = new();
    private readonly CbrXmlParser _sut;

    public CbrXmlParserTests()
    {
        _sut = new CbrXmlParser(_logger);
    }

    [Fact]
    public void Parse_SingleValute_ReturnsCurrency()
    {
        var doc = XDocument.Parse("""
            <ValCurs Date="02.04.2026" name="Foreign Currency Market">
                <Valute ID="R01235">
                    <NumCode>840</NumCode>
                    <CharCode>USD</CharCode>
                    <Nominal>1</Nominal>
                    <Name>Доллар США</Name>
                    <Value>94,6917</Value>
                </Valute>
            </ValCurs>
            """);

        var result = _sut.Parse(doc);

        result.Should().BeEquivalentTo([
            new Currency { CharCode = "USD", Name = "Доллар США", Rate = 94.6917m }
        ]);
        _logger.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Fact]
    public void Parse_NominalGreaterThanOne_DividesValueByNominal()
    {
        var doc = XDocument.Parse("""
            <ValCurs Date="02.04.2026" name="Foreign Currency Market">
                <Valute ID="R01090">
                    <NumCode>392</NumCode>
                    <CharCode>JPY</CharCode>
                    <Nominal>100</Nominal>
                    <Name>Японских иен</Name>
                    <Value>63,4920</Value>
                </Valute>
            </ValCurs>
            """);

        var result = _sut.Parse(doc);

        result.Should().BeEquivalentTo([
            new Currency { CharCode = "JPY", Name = "Японских иен", Rate = 0.634920m }
        ]);
        _logger.Collector.GetSnapshot().Should().BeEmpty();
    }

    [Fact]
    public void Parse_MissingCharCode_SkipsAndLogsWarning()
    {
        var doc = XDocument.Parse("""
            <ValCurs Date="02.04.2026" name="Foreign Currency Market">
                <Valute ID="R01235">
                    <NumCode>840</NumCode>
                    <Nominal>1</Nominal>
                    <Name>Доллар США</Name>
                    <Value>94,6917</Value>
                </Valute>
            </ValCurs>
            """);

        var result = _sut.Parse(doc);

        result.Should().BeEmpty();
        _logger.Collector.GetSnapshot().Should().ContainSingle(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void Parse_InvalidValue_SkipsAndLogsWarning()
    {
        var doc = XDocument.Parse("""
            <ValCurs Date="02.04.2026" name="Foreign Currency Market">
                <Valute ID="R01235">
                    <NumCode>840</NumCode>
                    <CharCode>USD</CharCode>
                    <Nominal>1</Nominal>
                    <Name>Доллар США</Name>
                    <Value>not_a_number</Value>
                </Valute>
            </ValCurs>
            """);

        var result = _sut.Parse(doc);

        result.Should().BeEmpty();
        _logger.Collector.GetSnapshot().Should().ContainSingle(e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public void Parse_MultipleRows_ReturnsAll()
    {
        var doc = XDocument.Parse("""
            <ValCurs Date="02.04.2026" name="Foreign Currency Market">
                <Valute ID="R01235">
                    <NumCode>840</NumCode>
                    <CharCode>USD</CharCode>
                    <Nominal>1</Nominal>
                    <Name>Доллар США</Name>
                    <Value>94,6917</Value>
                </Valute>
                <Valute ID="R01239">
                    <NumCode>978</NumCode>
                    <CharCode>EUR</CharCode>
                    <Nominal>1</Nominal>
                    <Name>Евро</Name>
                    <Value>103,4700</Value>
                </Valute>
            </ValCurs>
            """);

        var result = _sut.Parse(doc);

        result.Should().BeEquivalentTo([
            new Currency { CharCode = "USD", Name = "Доллар США", Rate = 94.6917m },
            new Currency { CharCode = "EUR", Name = "Евро", Rate = 103.4700m }
        ]);
        _logger.Collector.GetSnapshot().Should().BeEmpty();
    }
}

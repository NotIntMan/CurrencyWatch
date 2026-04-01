namespace Common.Domain.Entities;

public class Currency
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string CharCode { get; init; }
    public decimal Rate { get; set; }
}

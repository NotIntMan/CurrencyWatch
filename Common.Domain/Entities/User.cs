namespace Common.Domain.Entities;

public class User
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string PasswordHash { get; set; }
}

namespace Common.Domain.Entities;

public class RefreshToken
{
    public int Id { get; init; }
    public required int UserId { get; init; }
    public required string Token { get; set; }
    public required DateTime ExpiresAt { get; init; }

    public static RefreshToken Create(int userId) => new()
    {
        UserId = userId,
        Token = Guid.NewGuid().ToString(),
        ExpiresAt = DateTime.UtcNow.AddDays(7),
    };
}

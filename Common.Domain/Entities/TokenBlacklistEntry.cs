namespace Common.Domain.Entities;

public class TokenBlacklistEntry
{
    public required string Jti { get; init; }
    public required DateTime ExpiresAt { get; init; }
}


using eMarket.Domain.Identity;

namespace eMarket.Domain.Identity.Entities;

public sealed class RefreshToken
{
    private RefreshToken() { }

    public RefreshToken(
        Guid id,
        UserId userId,
        string tokenHash,
        DateTime createdAt,
        DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }

    public UserId UserId { get; private set; } = default!;

    public string TokenHash { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    public bool IsValid(DateTime utcNow) =>
        RevokedAt is null && ExpiresAt > utcNow;

    public void Revoke(
        DateTime utcNow,
        string? replacedByTokenHash = null)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = utcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}

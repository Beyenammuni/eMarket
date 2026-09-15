using eMarket.Domain.Identity;

namespace eMarket.Domain.Identity.Entities;

public sealed class PasswordResetToken
{
    private PasswordResetToken() { }

    public PasswordResetToken(
        Guid id,
        UserId userId,
        string tokenHash,
        DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public UserId UserId { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public bool IsValid(DateTime utcNow) => UsedAt is null && ExpiresAt > utcNow;

    public void MarkUsed() => UsedAt = DateTime.UtcNow;
}

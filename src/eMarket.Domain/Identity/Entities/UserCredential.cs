using eMarket.Domain.Identity;

namespace eMarket.Domain.Identity.Entities;

public sealed class UserCredential
{
    private UserCredential()
    {
    }

    public UserCredential(
        UserId userId,
        string passwordHash)
    {
        UserId = userId;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public UserId UserId { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
}

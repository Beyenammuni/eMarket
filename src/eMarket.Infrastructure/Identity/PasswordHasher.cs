using eMarket.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace eMarket.Infrastructure.Identity;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash)
        => _hasher.VerifyHashedPassword(null!, passwordHash, password)
            != PasswordVerificationResult.Failed;
}

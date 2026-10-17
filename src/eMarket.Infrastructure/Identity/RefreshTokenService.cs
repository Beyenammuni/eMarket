
using System.Security.Cryptography;
using System.Text;
using eMarket.Application.Common.Interfaces;

namespace eMarket.Infrastructure.Identity;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}

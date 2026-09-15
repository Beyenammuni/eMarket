using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;

namespace eMarket.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(
        UserId userId,
        string email,
        IEnumerable<string> roles,
        BusinessId? businessId = null,
        BusinessRole? businessRole = null);
}

namespace eMarket.Application.Common.Interfaces;

using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;

public interface ICurrentUser
{
    UserId? UserId { get; }

    BusinessId? BusinessId { get; }
    string? Email { get; }

    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
    BusinessRole? BusinessRole { get; }

    bool IsInRole(string role);
}

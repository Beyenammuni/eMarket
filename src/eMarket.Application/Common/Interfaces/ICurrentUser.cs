namespace eMarket.Application.Common.Interfaces;

using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;

public interface ICurrentUser
{
    UserId UserId { get; }

    BusinessId? BusinessId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}

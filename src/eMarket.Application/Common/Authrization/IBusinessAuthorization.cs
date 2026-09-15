using eMarket.Application.Common.Authorization;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;

namespace eMarket.Application.Common.Interfaces;

public interface IBusinessAuthorization
{
    Task<bool> IsMemberAsync(
        BusinessId businessId,
        UserId userId,
        CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync(
        BusinessId businessId,
        UserId userId,
        BusinessRole role,
        CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(
        BusinessId businessId,
        string permission,
        CancellationToken cancellationToken = default);
}

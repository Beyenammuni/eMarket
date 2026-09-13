using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;

namespace eMarket.Application.Common.IRepositories;

public interface IBusinessRepository
{
    Task<Business?> GetByIdAsync(
        BusinessId id,
        CancellationToken cancellationToken = default);

    Task<Business?> GetByNameAsync(
        BusinessName name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        BusinessName name,
           BusinessId? excludedId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Business Business,
        CancellationToken cancellationToken = default);
    Task<List<Business>> GetAllAsync(
    CancellationToken cancellationToken = default);

    Task<Business?> GetWithMembersAsync(
   BusinessId id,
   CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(
    BusinessId businessId,
    UserId userId,
    CancellationToken cancellationToken = default);

    Task<BusinessMember?> GetMemberAsync(
        BusinessId businessId,
        UserId userId,
        CancellationToken cancellationToken = default);
}

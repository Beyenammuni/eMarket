using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;

namespace eMarket.Application.Common.Interfaces;

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
}

using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;

namespace eMarket.Application.Common.IRepositories;

public interface IUserAddressRepository
{
    Task<List<UserAddress>> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task<UserAddress?> GetByIdAsync(
        Guid id,
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UserAddress address,
        CancellationToken cancellationToken = default);

    void Remove(UserAddress address);
}

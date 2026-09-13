using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Carts;

namespace eMarket.Application.Common.IRepositories;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(
        CartId id,
        CancellationToken cancellationToken = default);

    Task<Cart?> GetActiveByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Cart cart,
        CancellationToken cancellationToken = default);
}

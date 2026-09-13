using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Orders;

namespace eMarket.Application.Common.IRepositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(
        OrderId id,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdWithItemsAsync(
        OrderId id,
        CancellationToken cancellationToken = default);

    Task<List<Order>> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);
}

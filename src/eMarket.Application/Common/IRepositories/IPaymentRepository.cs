using eMarket.Domain.Payments;

namespace eMarket.Application.Common.IRepositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(
        PaymentId id,
        CancellationToken cancellationToken = default);

    Task<Payment?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default);
}

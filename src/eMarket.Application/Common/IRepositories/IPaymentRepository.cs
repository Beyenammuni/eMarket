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
    Task<Payment?> GetBySubscriptionIdAsync(
    Guid subscriptionId,
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetExpiredPendingAsync(
    DateTime now,
    int take,
    CancellationToken cancellationToken = default);

    Task<Payment?> GetByProviderPaymentIdAsync(
    string providerPaymentId,
    CancellationToken cancellationToken = default);
}

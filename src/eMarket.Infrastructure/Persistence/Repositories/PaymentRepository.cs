using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository
    : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(
            payment,
            cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(
        PaymentId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<Payment?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(
                x => x.OrderId == orderId,
                cancellationToken);
    }

    public async Task<Payment?> GetBySubscriptionIdAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(
                x => x.SubscriptionId == subscriptionId,
                cancellationToken);
    }
    public async Task<IReadOnlyList<Payment>> GetExpiredPendingAsync(
    DateTime now,
    int take,
    CancellationToken cancellationToken = default)
    {
        if (take <= 0)
            return Array.Empty<Payment>();
        return await _context.Payments
            .Where(x =>
                x.Status == PaymentStatus.Pending &&
                x.CheckoutToken != null &&
                x.CheckoutExpiresAt != null &&
                x.CheckoutExpiresAt <= now)
            .OrderBy(x => x.CheckoutExpiresAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
    public async Task<Payment?> GetByProviderPaymentIdAsync(
    string providerPaymentId,
    CancellationToken cancellationToken = default)
{
    return await _context.Payments.FirstOrDefaultAsync(
        x => x.ProviderPaymentId == providerPaymentId,
        cancellationToken);
}
}


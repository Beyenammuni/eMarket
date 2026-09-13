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
}

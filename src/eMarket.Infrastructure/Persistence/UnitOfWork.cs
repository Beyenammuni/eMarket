using eMarket.Application.Common.Interfaces;

namespace eMarket.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public Task ExecuteInSerializableTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
        => _context.ExecuteInSerializableTransactionAsync(
            operation,
            cancellationToken);
}

namespace eMarket.Application.Common.Interfaces;

public  interface IUnitOfWork
{
     Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
    Task ExecuteInSerializableTransactionAsync(
    Func<CancellationToken, Task> operation,
    CancellationToken cancellationToken = default);

}

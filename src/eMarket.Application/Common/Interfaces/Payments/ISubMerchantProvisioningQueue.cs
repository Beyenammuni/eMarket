using System.Runtime.CompilerServices;

namespace eMarket.Application.Common.Interfaces.Payments;

public interface ISubMerchantProvisioningQueue
{
    ValueTask QueueAsync(
        Guid businessId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<Guid> ReadAllAsync(
        CancellationToken cancellationToken = default);
}

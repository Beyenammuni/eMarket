using System.Threading.Channels;
using eMarket.Application.Common.Interfaces.Payments;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class SubMerchantProvisioningQueue
    : ISubMerchantProvisioningQueue
{
    private readonly Channel<Guid> _queue;

    public SubMerchantProvisioningQueue()
    {
        _queue = Channel.CreateUnbounded<Guid>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
    }

    public ValueTask QueueAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(
            businessId,
            cancellationToken);
    }

    public IAsyncEnumerable<Guid> ReadAllAsync(
        CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(
            cancellationToken);
    }
}


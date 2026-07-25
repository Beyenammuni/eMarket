using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}

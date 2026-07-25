using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using eMarket.SharedKernel.DomainEvent;
using eMarket.Application.Common.Interfaces;

namespace eMarket.Infrastructure.Persistence.Interceptors;

public sealed class PublishDomainEventsInterceptor
    : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;

    public PublishDomainEventsInterceptor(
        IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        var entities = eventData.Context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        await _dispatcher.DispatchAsync(
            domainEvents,
            cancellationToken);

        return await base.SavedChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}

using eMarket.Application.Common.Interfaces;
using eMarket.Application.Subscriptions.Commands.GenerateSubscriptionOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eMarket.Infrastructure.Subscriptions;

public sealed class SubscriptionOrderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionOrderBackgroundService> _logger;
    public SubscriptionOrderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SubscriptionOrderBackgroundService> logger) { _scopeFactory=scopeFactory; _logger=logger; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ISubscriptionDbContext>();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    var dueIds = await context.Subscriptions.AsNoTracking()
                        .Where(x => x.Status == eMarket.Domain.Subscriptions.SubscriptionStatus.Active && x.NextDeliveryDate.Date <= DateTime.UtcNow.Date)
                        .Select(x => x.Id.Value).Take(100).ToListAsync(stoppingToken);
                    foreach (var id in dueIds)
                    {
                        var result = await sender.Send(new GenerateSubscriptionOrderCommand(id), stoppingToken);
                        if (result.IsFailure) _logger.LogWarning("Subscription {SubscriptionId} was not processed: {Code} - {Description}", id, result.Error.Code, result.Error.Description);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
                catch (Exception ex) { _logger.LogError(ex, "Subscription order processing failed."); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected on shutdown - swallow.
        }
    }
}

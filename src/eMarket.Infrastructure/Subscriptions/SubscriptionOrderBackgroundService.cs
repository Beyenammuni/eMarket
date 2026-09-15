using eMarket.Application.Common.Interfaces;
using eMarket.Application.Subscriptions.Commands.GenerateSubscriptionOrder;
using eMarket.Domain.Subscriptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eMarket.Infrastructure.Subscriptions;

public sealed class SubscriptionOrderBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);
    private const int BatchSize = 100;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionOrderBackgroundService> _logger;

    public SubscriptionOrderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionOrderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        try
        {
            // Run once shortly after application startup.
            await ProcessDueSubscriptionsAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessDueSubscriptionsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Expected during application shutdown.
        }
    }

    private async Task ProcessDueSubscriptionsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider.GetRequiredService<ISubscriptionDbContext>();

            var sender =
                scope.ServiceProvider.GetRequiredService<ISender>();

            // NextDeliveryDate is stored as a date (midnight).
            // Using a range instead of .Date keeps the query SARGable
            // and allows SQL Server to use the index on NextDeliveryDate.
            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            var dueIds = await context.Subscriptions
                .AsNoTracking()
                .Where(x =>
                    x.Status == SubscriptionStatus.Active &&
                    x.NextDeliveryDate < tomorrowUtc)
                .OrderBy(x => x.NextDeliveryDate)
                .Select(x => x.Id.Value)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (dueIds.Count == 0)
            {
                return;
            }

            _logger.LogInformation(
                "Processing {Count} due subscription(s).",
                dueIds.Count);

            foreach (var subscriptionId in dueIds)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var result = await sender.Send(
                        new GenerateSubscriptionOrderCommand(subscriptionId),
                        cancellationToken);

                    if (result.IsFailure)
                    {
                        _logger.LogWarning(
                            "Subscription {SubscriptionId} was not processed: {Code} - {Description}",
                            subscriptionId,
                            result.Error.Code,
                            result.Error.Description);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Subscription {SubscriptionId} order generated successfully.",
                            subscriptionId);
                    }
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process subscription {SubscriptionId}.",
                        subscriptionId);
                }
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Subscription order processing failed.");
        }
    }
}

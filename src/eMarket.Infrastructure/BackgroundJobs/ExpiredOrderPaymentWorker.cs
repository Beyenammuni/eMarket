using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Sales.Orders.Services;
using eMarket.Application.Sales.Payments.Services;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eMarket.Infrastructure.BackgroundJobs;

public sealed class ExpiredOrderPaymentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredOrderPaymentWorker> _logger;

    public ExpiredOrderPaymentWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredOrderPaymentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredPaymentsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error while processing expired order payments.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessExpiredPaymentsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var paymentRepository = scope.ServiceProvider
            .GetRequiredService<IPaymentRepository>();

        var confirmationService = scope.ServiceProvider
            .GetRequiredService<OrderPaymentConfirmationService>();

        var stockReleaseService = scope.ServiceProvider
            .GetRequiredService<OrderStockReleaseService>();

        var paymentGateway = scope.ServiceProvider
            .GetRequiredService<IPaymentGateway>();

        var orderRepository = scope.ServiceProvider
            .GetRequiredService<IOrderRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var payments = await paymentRepository.GetExpiredPendingAsync(
            DateTime.UtcNow,
            50,
            cancellationToken);

        foreach (var payment in payments)
        {
            try
            {
                if (payment.OrderId is null ||
                    string.IsNullOrWhiteSpace(payment.CheckoutToken))
                {
                    continue;
                }

                var order = await orderRepository.GetByIdWithItemsAsync(
                    OrderId.Create(payment.OrderId.Value),
                    cancellationToken);

                if (order is null)
                    continue;

                var checkout = await paymentGateway.GetCheckoutResultAsync(
                    payment.CheckoutToken,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(checkout.PaymentId))
                {
                    var providerIdResult =
                        payment.SetProviderPaymentId(checkout.PaymentId);

                    if (providerIdResult.IsFailure)
                    {
                        _logger.LogWarning(
                            "Could not save iyzico payment ID for payment {PaymentId}.",
                            payment.Id.Value);

                        continue;
                    }

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }

                if (checkout.VerificationStatus ==
                        PaymentVerificationStatus.Pending &&
                    !string.IsNullOrWhiteSpace(payment.ProviderPaymentId))
                {
                    var paymentResult =
                        await paymentGateway.GetPaymentResultAsync(
                            payment.ProviderPaymentId,
                            payment.Id.Value.ToString(),
                            cancellationToken);

                    if (paymentResult.VerificationStatus !=
                        PaymentVerificationStatus.Unknown)
                    {
                        checkout = paymentResult;
                    }
                }

                if (!string.Equals(
                        checkout.ConversationId,
                        payment.Id.Value.ToString(),
                        StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(
                        checkout.BasketId,
                        order.Id.Value.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Payment references mismatch for payment {PaymentId}.",
                        payment.Id.Value);

                    continue;
                }

                if (checkout.VerificationStatus ==
                    PaymentVerificationStatus.Succeeded)
                {
                    var result = await confirmationService.ConfirmAsync(
                        payment,
                        payment.CheckoutToken,
                        cancellationToken,
                        checkout);

                    if (result.IsFailure)
                    {
                        _logger.LogWarning(
                            "Could not confirm payment {PaymentId}: {Error}.",
                            payment.Id.Value,
                            result.Error);
                    }

                    continue;
                }

                if (checkout.VerificationStatus ==
                    PaymentVerificationStatus.Failed)
                {
                    var failureResult = payment.MarkAsFailed();

                    if (failureResult.IsFailure)
                        continue;

                    var releaseResult =
                        await stockReleaseService.ReleaseAsync(
                            order,
                            cancellationToken);
                    if (releaseResult.IsFailure)
                    {
                        _logger.LogWarning(
                            "Could not release stock for order {OrderId}.",
                            order.Id.Value);

                        return;
                    }

                    continue;
                }

                // Pending or Unknown:
                // Keep the order and reserved stock unchanged.
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to process expired payment {PaymentId}.",
                    payment.Id.Value);
            }
        }
    }
}

using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Sales.Orders.Services;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace eMarket.Application.Sales.Payments.Services;

public sealed class OrderPaymentConfirmationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderStockReleaseService _stockReleaseService;
    private readonly ILogger<OrderPaymentConfirmationService> _logger;

    public OrderPaymentConfirmationService(
        IOrderRepository orderRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        OrderStockReleaseService stockReleaseService,
        ILogger<OrderPaymentConfirmationService> logger)
    {
        _orderRepository = orderRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _stockReleaseService = stockReleaseService;
        _logger = logger;
    }

    public async Task<Result> ConfirmAsync(
    Payment payment,
    string token,
    CancellationToken cancellationToken = default,
    PaymentCheckoutResult? verifiedResult = null)
    {
        if (payment.OrderId is null ||
            string.IsNullOrWhiteSpace(payment.CheckoutToken) ||
            !string.Equals(
                payment.CheckoutToken,
                token,
                StringComparison.Ordinal))
        {
            return Result.Failure(PaymentErrors.InvalidStatus);
        }

        if (payment.Status == PaymentStatus.Succeeded)
            return Result.Success();

        if (payment.Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.InvalidStatus);

        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(payment.OrderId.Value),
            cancellationToken);

        if (order is null)
            return Result.Failure(OrderErrors.NotFound);

        var checkout = verifiedResult ??
    await _paymentGateway.GetCheckoutResultAsync(
        payment.CheckoutToken, cancellationToken);

        _logger.LogInformation(
    """
    iyzico Checkout Verification:
    Expected ConversationId: {ExpectedConversationId}
    Actual ConversationId: {ActualConversationId}
    Expected BasketId: {ExpectedBasketId}
    Actual BasketId: {ActualBasketId}
    VerificationStatus: {VerificationStatus}
    """,
    payment.Id.Value,
    checkout.ConversationId,
    order.Id.Value,
    checkout.BasketId,
    checkout.VerificationStatus);
        // Verify references before handling any payment status.
        if (
      (!string.IsNullOrWhiteSpace(checkout.ConversationId) &&
       !string.Equals(
           checkout.ConversationId,
           payment.Id.Value.ToString(),
           StringComparison.OrdinalIgnoreCase))
      ||
      !string.Equals(
          checkout.BasketId,
          order.Id.Value.ToString(),
          StringComparison.OrdinalIgnoreCase)
  )
        {
            return Result.Failure(
                new Error(
                    "Payment.ReferenceMismatch",
                    "The payment references do not match."));
        }
        // Payment failed: mark it failed and release reserved stock.
        if (checkout.VerificationStatus == PaymentVerificationStatus.Failed)
        {
            var failedResult = payment.MarkAsFailed();

            if (failedResult.IsFailure)
                return failedResult;

            return await _stockReleaseService.ReleaseAsync(
                order,
                cancellationToken);
        }

        // Pending or Unknown: keep stock reserved.
        if (checkout.VerificationStatus != PaymentVerificationStatus.Succeeded ||
            string.IsNullOrWhiteSpace(checkout.PaymentId))
        {
            return Result.Failure(
                new Error(
                    "Payment.NotVerified",
                    "The payment has not been verified as successful."));
        }

        if (!decimal.TryParse(
                checkout.Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price) ||
            !decimal.TryParse(
                checkout.PaidPrice,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var paidPrice))
        {
            return Result.Failure(
                new Error(
                    "Payment.InvalidAmount",
                    "The payment amount returned by iyzico is invalid."));
        }

        var expectedAmount = payment.Amount.Amount;

        if (decimal.Round(price, 2) != decimal.Round(expectedAmount, 2) ||
            decimal.Round(paidPrice, 2) != decimal.Round(expectedAmount, 2))
        {
            return Result.Failure(
                new Error(
                    "Payment.AmountMismatch",
                    "The payment amount does not match."));
        }

        if (!string.Equals(
                checkout.Currency,
                payment.Amount.Currency.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                new Error(
                    "Payment.CurrencyMismatch",
                    "The payment currency does not match."));
        }
        if (!string.IsNullOrWhiteSpace(payment.ProviderPaymentId) &&
    !string.Equals(
        payment.ProviderPaymentId,
        checkout.PaymentId,
        StringComparison.Ordinal))
        {
            return Result.Failure(
                new Error(
                    "Payment.ProviderPaymentIdMismatch",
                    "The provider payment ID does not match."));
        }
        var paymentResult = payment.MarkAsSucceeded(
            "iyzico",
            checkout.PaymentId);

        if (paymentResult.IsFailure)
            return paymentResult;

        var orderResult = order.MarkAsPaid();

        if (orderResult.IsFailure)
            return orderResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using System.Globalization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Sales.Payments.Services;

public sealed class OrderPaymentConfirmationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public OrderPaymentConfirmationService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
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
       
        if (checkout.VerificationStatus !=
                PaymentVerificationStatus.Succeeded ||
            string.IsNullOrWhiteSpace(checkout.PaymentId))
        {
            return Result.Failure(
                new Error(
                    "Payment.NotVerified",
                    "The payment has not been verified as successful."));
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
            return Result.Failure(
                new Error(
                    "Payment.ReferenceMismatch",
                    "The payment references do not match."));
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

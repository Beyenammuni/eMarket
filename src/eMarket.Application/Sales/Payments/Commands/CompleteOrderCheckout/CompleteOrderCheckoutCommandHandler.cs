using System.Globalization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Commands.CompleteOrderCheckout;

internal sealed class CompleteOrderCheckoutCommandHandler
    : IRequestHandler<CompleteOrderCheckoutCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOrderCheckoutCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CompleteOrderCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            PaymentId.Create(request.PaymentId),
            cancellationToken);

        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound);
        }

        if (payment.OrderId is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(payment.OrderId.Value),
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var checkoutResult =
            await _paymentGateway.GetCheckoutResultAsync(
                request.Token,
                cancellationToken);

        if (!checkoutResult.IsSuccess ||
     string.IsNullOrWhiteSpace(checkoutResult.PaymentId))
        {
            return Result.Failure(
                new Error(
                    "Payment.NotSuccessful",
                    checkoutResult.Error ??
                    "iyzico payment was not successful."));
        }

        if (!string.Equals(
                checkoutResult.ConversationId,
                payment.Id.Value.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                new Error(
                    "Payment.ConversationMismatch",
                    "The iyzico payment does not match the stored payment."));
        }

        // The verified iyzico basket must belong to this order.
        if (!string.Equals(
                checkoutResult.BasketId,
                order.Id.Value.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                new Error(
                    "Payment.OrderMismatch",
                    "The iyzico payment does not match this order."));
        }

        // Both the requested price and the actually paid price
        // must match the amount stored for this payment.
        if (!decimal.TryParse(
                checkoutResult.Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price) ||
            !decimal.TryParse(
                checkoutResult.PaidPrice,
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
                    "The iyzico payment amount does not match the stored payment."));
        }

        if (!string.Equals(
                checkoutResult.Currency,
                payment.Amount.Currency.ToString(),
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(
                new Error(
                    "Payment.CurrencyMismatch",
                    "The iyzico payment currency does not match the stored payment."));
        }

        var paymentResult = payment.MarkAsSucceeded(
            "iyzico",
            checkoutResult.PaymentId);

        if (paymentResult.IsFailure)
        {
            return paymentResult;
        }

        var orderResult = order.MarkAsPaid();

        if (orderResult.IsFailure)
        {
            return orderResult;
        }

        var cart = await _cartRepository.GetActiveByUserAsync(
            order.UserId,
            cancellationToken);

        if (cart is not null &&
            cart.Status == CartStatus.Active &&
            cart.Items.Any())
        {
            var cartCheckoutResult = cart.Checkout();

            if (cartCheckoutResult.IsFailure)
            {
                return cartCheckoutResult;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

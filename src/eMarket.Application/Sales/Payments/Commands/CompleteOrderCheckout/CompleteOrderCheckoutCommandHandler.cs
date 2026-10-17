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
            return Result.Failure(
                PaymentErrors.NotFound);
        }

        if (payment.OrderId is null)
        {
            return Result.Failure(
                OrderErrors.NotFound);
        }

        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(payment.OrderId.Value),
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(
                OrderErrors.NotFound);
        }

        var checkoutResult =
            await _paymentGateway.GetCheckoutResultAsync(
                request.Token,
                cancellationToken);

        if (!checkoutResult.IsSuccess ||
            string.IsNullOrWhiteSpace(
                checkoutResult.PaymentId))
        {
            return Result.Failure(
                new Error(
                    "Payment.NotSuccessful",
                    checkoutResult.Error ??
                    "iyzico payment was not successful."));
        }

        var paymentResult =
            payment.MarkAsSucceeded(
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

        if (cart is not null && cart.Status == CartStatus.Active)
        {
            if (cart.Items.Any())
            {
                var cartCheckoutResult = cart.Checkout();

                if (cartCheckoutResult.IsFailure)
                {
                    return cartCheckoutResult;
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

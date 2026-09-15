using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Commands.ProcessPaymentWebhook;

internal sealed class ProcessPaymentWebhookCommandHandler
    : IRequestHandler<ProcessPaymentWebhookCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ProcessPaymentWebhookCommand request,
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

        if (!request.Succeeded)
        {
            var failedResult = payment.MarkAsFailed();

            if (failedResult.IsFailure)
                return failedResult;

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }

        var paymentResult = payment.MarkAsSucceeded(
            request.Provider,
            request.ProviderPaymentId);

        if (paymentResult.IsFailure)
            return paymentResult;

        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(payment.OrderId),
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(
                OrderErrors.NotFound);
        }

        var orderResult = order.MarkAsPaid();

        if (orderResult.IsFailure)
            return orderResult;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

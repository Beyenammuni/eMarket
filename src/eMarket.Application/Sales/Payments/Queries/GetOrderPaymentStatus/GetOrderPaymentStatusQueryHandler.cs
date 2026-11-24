using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Queries.GetOrderPaymentStatus;

internal sealed class GetOrderPaymentStatusQueryHandler
    : IRequestHandler<GetOrderPaymentStatusQuery, Result<OrderPaymentStatusResponse>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetOrderPaymentStatusQueryHandler(
        ICurrentUser currentUser,
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository)
    {
        _currentUser = currentUser;
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<OrderPaymentStatusResponse>> Handle(
        GetOrderPaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Result<OrderPaymentStatusResponse>.Failure(
          PaymentErrors.NotFound);


        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(request.OrderId),
            cancellationToken);

        if (order is null || order.UserId != _currentUser.UserId)
            return Result<OrderPaymentStatusResponse>.Failure(
      PaymentErrors.NotFound);


        var payment = await _paymentRepository.GetByOrderIdAsync(
            request.OrderId,
            cancellationToken);

        if (payment is null)
            return Result<OrderPaymentStatusResponse>.Failure(
      PaymentErrors.NotFound);

        return Result<OrderPaymentStatusResponse>.Success(new OrderPaymentStatusResponse(
            request.OrderId,
            payment.Status.ToString(),
            order.Status.ToString()));
    }
}

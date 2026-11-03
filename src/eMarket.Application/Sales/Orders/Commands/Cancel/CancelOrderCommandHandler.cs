using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Sales.Orders.Services;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.Cancel;

internal sealed class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderStockReleaseService _stockReleaseService;
    private readonly ICurrentUser _currentUser;
    private readonly IPaymentRepository _paymentRepository;
    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        IPaymentRepository paymentRepository,
        OrderStockReleaseService stockReleaseService)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _paymentRepository = paymentRepository;
        _stockReleaseService = stockReleaseService;
    }

    public async Task<Result> Handle(
    CancelOrderCommand request,
    CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(
            OrderId.Create(request.Id),
            cancellationToken);

        if (order is null || order.UserId != _currentUser.UserId)
            return Result.Failure(OrderErrors.NotFound);

        var payment = await _paymentRepository.GetByOrderIdAsync(
            request.Id,
            cancellationToken);

        if (payment is not null &&
            payment.Status != PaymentStatus.Failed &&
            payment.Status != PaymentStatus.Cancelled)
        {
            return Result.Failure(PaymentErrors.InvalidStatus);
        }

        return await _stockReleaseService.ReleaseAsync(
            order,
            cancellationToken);
    }
}

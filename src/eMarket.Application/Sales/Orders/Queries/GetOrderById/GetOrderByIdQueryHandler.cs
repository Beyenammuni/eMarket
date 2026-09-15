using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdHandler
    : IRequestHandler<
        GetOrderByIdQuery,
        Result<GetOrderByIdResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;

    public GetOrderByIdHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<GetOrderByIdResponse>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(
            OrderId.Create(request.Id),
            cancellationToken);

        if (order is null)
        {
            return Result<GetOrderByIdResponse>.Failure(
                OrderErrors.NotFound);
        }

        if (order.UserId != _currentUser.UserId)
        {
            return Result<GetOrderByIdResponse>.Failure(
                OrderErrors.NotFound);
        }

        var items = order.Items
            .Select(x =>
                new OrderItemResponse(
                    x.ProductId.Value,
                    x.ProductName,
                    x.UnitPrice.Amount,
                    x.UnitPrice.Currency.ToString(),
                    x.Quantity,
                    x.TotalPrice))
            .ToList();

        return Result<GetOrderByIdResponse>.Success(
            new GetOrderByIdResponse(
                order.Id.Value,
                order.UserId.Value,
                order.Status.ToString(),
                order.CreatedAt,
                order.UpdatedAt,
                order.TotalAmount,
                items));
    }
}

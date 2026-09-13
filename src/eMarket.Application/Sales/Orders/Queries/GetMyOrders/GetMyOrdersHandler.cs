using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetMyOrders;

internal sealed class GetMyOrdersHandler
    : IRequestHandler<
        GetMyOrdersQuery,
        Result<List<GetMyOrdersResponse>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;

    public GetMyOrdersHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<List<GetMyOrdersResponse>>> Handle(
        GetMyOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(
            _currentUser.UserId,
            cancellationToken);

        var response = orders
            .Select(order =>
                new GetMyOrdersResponse(
                    order.Id.Value,
                    order.Status.ToString(),
                    order.CreatedAt,
                    order.TotalAmount,
                    order.Items.Sum(x => x.Quantity)))
            .ToList();

        return Result<List<GetMyOrdersResponse>>.Success(
            response);
    }
}

using eMarket.Application.Common.IRepositories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrders;

internal sealed class GetBusinessOrdersHandler
    : IRequestHandler<
        GetBusinessOrdersQuery,
        Result<List<GetBusinessOrdersResponse>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public GetBusinessOrdersHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<List<GetBusinessOrdersResponse>>> Handle(
        GetBusinessOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByBusinessIdAsync(
            request.BusinessId,
            cancellationToken);

        var response = new List<GetBusinessOrdersResponse>();

        foreach (var order in orders)
        {
            var user = await _userRepository.GetByIdAsync(
                order.UserId,
                cancellationToken);

            response.Add(new GetBusinessOrdersResponse(
                order.Id.Value,
                user?.FullName.ToString() ?? "Müşteri",
                user?.Username ?? string.Empty,
                order.Status.ToString(),
                order.CreatedAt,
                order.TotalAmount,
                order.Items.Sum(x => x.Quantity),
                order.Items
                    .Select(x => new BusinessOrderItemResponse(
                        x.ProductId.Value,
                        x.ProductName,
                        x.Quantity,
                        x.UnitPrice.Amount,
                        x.TotalPrice))
                    .ToList()));
        }

        return Result<List<GetBusinessOrdersResponse>>.Success(
            response);
    }
}

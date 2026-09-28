using eMarket.Application.Common.IRepositories;
using eMarket.Application.Sales.Orders.Queries.GetOrderById;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrderById;

internal sealed class GetBusinessOrderByIdHandler
    : IRequestHandler<
        GetBusinessOrderByIdQuery,
        Result<GetBusinessOrderByIdResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    public GetBusinessOrderByIdHandler(
        IOrderRepository orderRepository, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<GetBusinessOrderByIdResponse>> Handle(
        GetBusinessOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(
            new Domain.Sales.Orders.OrderId(request.OrderId),
            cancellationToken);

        if (order is null)
        {
            return Result<GetBusinessOrderByIdResponse>.Failure(
                OrderErrors.NotFound);
        }

        var user = await _userRepository.GetByIdAsync(
    order.UserId,
    cancellationToken);
        if (user is null)
        {
            return Result<GetBusinessOrderByIdResponse>.Failure(
                OrderErrors.NotFound);
        }

        if (order.BusinessId != request.BusinessId)
        {
            return Result<GetBusinessOrderByIdResponse>.Failure(
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

        return Result<GetBusinessOrderByIdResponse>.Success(
        new GetBusinessOrderByIdResponse(
            order.Id.Value,
            order.UserId.Value,
            user.FullName.ToString(),
            user.Username,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.TotalAmount,
            new DeliveryAddressResponse(
                order.DeliveryAddress.FullName,
                order.DeliveryAddress.PhoneNumber,
                order.DeliveryAddress.AddressLine,
                order.DeliveryAddress.City,
                order.DeliveryAddress.District,
                order.DeliveryAddress.PostalCode,
                order.DeliveryAddress.Neighborhood,
                order.DeliveryAddress.Street,
                order.DeliveryAddress.BuildingNumber,
                order.DeliveryAddress.ApartmentNumber,
                order.DeliveryAddress.Latitude,
                order.DeliveryAddress.Longitude),
            items));
    }
}

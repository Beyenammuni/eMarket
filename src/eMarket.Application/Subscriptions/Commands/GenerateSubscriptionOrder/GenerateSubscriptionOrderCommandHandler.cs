using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.ValueObjects;
using eMarket.Domain.Subscriptions;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Subscriptions.Commands.GenerateSubscriptionOrder;

internal sealed class GenerateSubscriptionOrderCommandHandler
    : IRequestHandler<
        GenerateSubscriptionOrderCommand,
        Result<GenerateSubscriptionOrderResponse>>
{
    private readonly ISubscriptionDbContext _context;
    private readonly ICatalogDbContext _catalog;
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _uow;

    public GenerateSubscriptionOrderCommandHandler(
        ISubscriptionDbContext context,
        ICatalogDbContext catalog,
        IOrderRepository orders,
        IUnitOfWork uow)
    {
        _context = context;
        _catalog = catalog;
        _orders = orders;
        _uow = uow;
    }

    public async Task<Result<GenerateSubscriptionOrderResponse>> Handle(
        GenerateSubscriptionOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get subscription
        var subscription = await _context.Subscriptions
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == SubscriptionId.Create(request.SubscriptionId),
                cancellationToken);

        if (subscription is null)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                SubscriptionErrors.NotFound);
        }

        // 2. Subscription must be active
        if (subscription.Status != SubscriptionStatus.Active)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                SubscriptionErrors.NotActive);
        }

        // 3. Delivery date must be due
        if (subscription.NextDeliveryDate.Date > DateTime.UtcNow.Date)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                new Error(
                    "Subscription.NotDue",
                    "The subscription is not due yet."));
        }

        // 4. Subscription must contain items
        if (subscription.Items.Count == 0)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                SubscriptionErrors.EmptyBasket);
        }

        // 5. Get active products belonging to the same business
        var productIds = subscription.Items
            .Select(x => x.ProductId)
            .ToList();

        var products = await _catalog.Products
            .Where(x =>
                productIds.Contains(x.Id) &&
                x.BusinessId == subscription.BusinessId &&
                x.Status == ProductStatus.Active)
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                SubscriptionErrors.DifferentBusinesses);
        }

        // 6. Create a new Order delivery-address snapshot
        var addressResult = DeliveryAddress.Create(
            subscription.DeliveryAddress.FullName,
            subscription.DeliveryAddress.PhoneNumber,
            subscription.DeliveryAddress.AddressLine,
            subscription.DeliveryAddress.City,
            subscription.DeliveryAddress.District,
            subscription.DeliveryAddress.PostalCode,
            subscription.DeliveryAddress.Neighborhood,
            subscription.DeliveryAddress.Street,
            subscription.DeliveryAddress.BuildingNumber,
            subscription.DeliveryAddress.ApartmentNumber,
            subscription.DeliveryAddress.Latitude,
            subscription.DeliveryAddress.Longitude);

        if (addressResult.IsFailure)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                addressResult.Error);
        }

        var deliveryAddress = addressResult.Value!;

        // 7. Create Order
        var orderResult = Order.Create(
            subscription.UserId,
            subscription.BusinessId,
            deliveryAddress);

        if (orderResult.IsFailure)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                orderResult.Error);
        }

        var order = orderResult.Value!;

        // 8. Add subscription items to Order
        foreach (var subscriptionItem in subscription.Items)
        {
            var product = products.First(
                x => x.Id == subscriptionItem.ProductId);

            // Check stock
            if (product.StockQuantity < subscriptionItem.Quantity)
            {
                return Result<GenerateSubscriptionOrderResponse>.Failure(
                    new Error(
                        "Subscription.InsufficientStock",
                        $"Insufficient stock for product {product.Id.Value}."));
            }

            // Add product snapshot to Order
            var addItemResult = order.AddItem(
                product.Id,
                product.Name.Value,
                product.Price,
                subscriptionItem.Quantity);

            if (addItemResult.IsFailure)
            {
                return Result<GenerateSubscriptionOrderResponse>.Failure(
                    addItemResult.Error);
            }

            // Decrease stock
            var decreaseStockResult = product.DecreaseStock(
                subscriptionItem.Quantity);

            if (decreaseStockResult.IsFailure)
            {
                return Result<GenerateSubscriptionOrderResponse>.Failure(
                    decreaseStockResult.Error);
            }
        }

        // 9. Record generated Order on Subscription
        var recordResult = subscription.RecordOrderGenerated(
            order.Id.Value);

        if (recordResult.IsFailure)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                recordResult.Error);
        }

        // 10. Persist everything
        await _orders.AddAsync(
            order,
            cancellationToken);

        await _uow.SaveChangesAsync(
            cancellationToken);

        // 11. Response
        return Result<GenerateSubscriptionOrderResponse>.Success(
            new GenerateSubscriptionOrderResponse(
                subscription.Id.Value,
                order.Id.Value,
                subscription.NextDeliveryDate));
    }
}

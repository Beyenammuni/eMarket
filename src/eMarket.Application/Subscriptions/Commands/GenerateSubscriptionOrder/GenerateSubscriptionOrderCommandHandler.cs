using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Inventory.Entities;
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
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessDbContext _businessDbContext;
    public GenerateSubscriptionOrderCommandHandler(
        ISubscriptionDbContext context,
        ICatalogDbContext catalog,
        IOrderRepository orders,
        IUnitOfWork uow,
        IBusinessRepository businessRepository,
        IBusinessDbContext businessDbContext)
    {
        _context = context;
        _catalog = catalog;
        _orders = orders;
        _uow = uow;
        _businessRepository = businessRepository;
        _businessDbContext = businessDbContext;
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
        var business = await _businessRepository.GetByIdAsync(
    subscription.BusinessId,
    cancellationToken);

        if (business is null)
        {
            return Result<GenerateSubscriptionOrderResponse>.Failure(
                new Error(
                    "Order.BusinessNotFound",
                    "The business could not be found."));
        }
        var subtotal = products.Sum(x =>
    x.Price.Amount *
    subscription.Items
        .First(i => i.ProductId == x.Id)
        .Quantity);

        var shippingFee =
            business.FreeShippingThreshold.HasValue &&
            subtotal >= business.FreeShippingThreshold.Value
                ? 0m
                : business.ShippingFee;


        var requiredItems = subscription.Items
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToList();

        var transactionResult =
            Result<GenerateSubscriptionOrderResponse>.Failure(
                new Error("Subscription.InsufficientStock", "Insufficient stock."));

        await _businessDbContext.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                var branches = await _businessDbContext.Branches
                    .Where(x =>
                        x.BusinessId == subscription.BusinessId &&
                        x.IsActive)
                    .ToListAsync(ct);

                if (branches.Count == 0)
                {
                    transactionResult =
                        Result<GenerateSubscriptionOrderResponse>.Failure(
                            new Error(
                                "Order.NoAvailableBranch",
                                "No active branch is available."));
                    return;
                }

                var branchIds = branches.Select(x => x.Id).ToList();

                var inventories = await _businessDbContext.BranchInventories
                    .Where(x =>
                        branchIds.Contains(x.BranchId) &&
                        requiredItems.Select(i => i.ProductId).Contains(x.ProductId) &&
                        x.VariantId == null)
                    .ToListAsync(ct);

                bool HasRequiredStock(Branch branch)
                {
                    return requiredItems.All(item =>
                        inventories.Any(inventory =>
                            inventory.BranchId == branch.Id &&
                            inventory.ProductId == item.ProductId &&
                            inventory.VariantId == null &&
                            inventory.AvailableQuantity >= item.Quantity));
                }

                var eligibleBranches = branches
                    .Where(HasRequiredStock)
                    .ToList();

                Branch? selectedBranch = null;

                if (deliveryAddress.Latitude.HasValue &&
                    deliveryAddress.Longitude.HasValue)
                {
                    var branchesWithCoordinates = eligibleBranches
                        .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
                        .ToList();

                    if (branchesWithCoordinates.Count > 0)
                    {
                        selectedBranch = branchesWithCoordinates
                            .OrderBy(x => GetDistanceInKm(
                                (double)deliveryAddress.Latitude.Value,
                                (double)deliveryAddress.Longitude.Value,
                                (double)x.Latitude!.Value,
                                (double)x.Longitude!.Value))
                            .First();
                    }
                }

                selectedBranch ??= eligibleBranches
                    .FirstOrDefault(x => x.IsMain);

                if (selectedBranch is null)
                {
                    transactionResult =
                        Result<GenerateSubscriptionOrderResponse>.Failure(
                            new Error(
                                "Subscription.InsufficientStock",
                                "No branch has all required items in stock."));
                    return;
                }

                var orderResult = Order.Create(
                    subscription.UserId,
                    subscription.BusinessId,
                    selectedBranch.Id,
                    deliveryAddress,
                    shippingFee);

                if (orderResult.IsFailure)
                {
                    transactionResult =
                        Result<GenerateSubscriptionOrderResponse>.Failure(
                            orderResult.Error);
                    return;
                }

                var order = orderResult.Value!;

                // Add all order item snapshots first.
                foreach (var subscriptionItem in subscription.Items)
                {
                    var product = products.First(
                        x => x.Id == subscriptionItem.ProductId);

                    var originalUnitPrice = product.Price;

                    var discountAmount =
                        Money.Create(0, originalUnitPrice.Currency);

                    var addItemResult = order.AddItem(
                        product.Id,
                        product.Name.Value,
                        originalUnitPrice,
                        discountAmount,
                        originalUnitPrice,
                        subscriptionItem.Quantity);

                    if (addItemResult.IsFailure)
                    {
                        transactionResult =
                            Result<GenerateSubscriptionOrderResponse>.Failure(
                                addItemResult.Error);
                        return;
                    }
                }

                var recordResult = subscription.RecordOrderGenerated(
                    order.Id.Value);

                if (recordResult.IsFailure)
                {
                    transactionResult =
                        Result<GenerateSubscriptionOrderResponse>.Failure(
                            recordResult.Error);
                    return;
                }

                // Deduct stock from the selected branch.
                foreach (var requirement in requiredItems)
                {
                    var inventory = inventories.First(
                        x => x.BranchId == selectedBranch.Id &&
                             x.ProductId == requirement.ProductId &&
                             x.VariantId == null);

                    inventory.DecreaseStock(requirement.Quantity);
                }

                await _orders.AddAsync(order, ct);
                await _uow.SaveChangesAsync(ct);

                transactionResult =
                    Result<GenerateSubscriptionOrderResponse>.Success(
                        new GenerateSubscriptionOrderResponse(
                            subscription.Id.Value,
                            order.Id.Value,
                            subscription.NextDeliveryDate));
            },
            cancellationToken);

        return transactionResult;
    }
    private static double GetDistanceInKm(
    double latitude1,
    double longitude1,
    double latitude2,
    double longitude2)
    {
        const double earthRadiusKm = 6371;

        var lat1 = latitude1 * Math.PI / 180;
        var lat2 = latitude2 * Math.PI / 180;

        var deltaLat = (latitude2 - latitude1) * Math.PI / 180;
        var deltaLon = (longitude2 - longitude1) * Math.PI / 180;

        var a =
            Math.Pow(Math.Sin(deltaLat / 2), 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLon / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }
}

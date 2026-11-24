using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Models;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Sales.Orders.Commands.CreateOrder;

internal sealed class CreateOrderCommandHandler
: IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessDbContext _businessDbContext;
    private readonly IPromotionPricingService _promotionPricingService;
    public CreateOrderCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IBusinessRepository businessRepository,
    IBusinessDbContext businessDbContext,
    IPromotionPricingService promotionPricingService)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _businessRepository = businessRepository;
        _businessDbContext = businessDbContext;
        _promotionPricingService = promotionPricingService;
    }


    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var cart = await _cartRepository.GetActiveByUserAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            return Result<CreateOrderResponse>.Failure(
                OrderErrors.CartNotFound);
        }

        if (!cart.Items.Any())
        {
            return Result<CreateOrderResponse>.Failure(
                OrderErrors.EmptyOrder);
        }

        // Create delivery address snapshot
        var addressResult = DeliveryAddress.Create(
            request.FullName,
            request.PhoneNumber,
            request.AddressLine,
            request.City,
            request.District,
            request.PostalCode,
            request.Neighborhood,
            request.Street,
            request.BuildingNumber,
            request.ApartmentNumber,
            request.Latitude,
            request.Longitude);

        if (addressResult.IsFailure)
        {
            return Result<CreateOrderResponse>.Failure(
                addressResult.Error);
        }

        var deliveryAddress = addressResult.Value!;

        // Load products and validate the cart
        var products = new List<Product>();

        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                cartItem.ProductId,
                cancellationToken);

            if (product is null)
            {
                return Result<CreateOrderResponse>.Failure(
                    OrderErrors.ProductNotFound);
            }

            if (product.Status != ProductStatus.Active ||
                product.IsDeleted)
            {
                return Result<CreateOrderResponse>.Failure(
                    OrderErrors.ProductNotAvailable);
            }

            if (cartItem.VariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(
                    x => x.Id == cartItem.VariantId.Value &&
                         x.IsActive);

                if (variant is null)
                {
                    return Result<CreateOrderResponse>.Failure(
                        OrderErrors.ProductNotAvailable);
                }
            }

            products.Add(product);
        }

        // Order must belong to one Business
        var businessIds = products
            .Select(x => x.BusinessId)
            .Distinct()
            .ToList();

        if (businessIds.Count == 0)
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.BusinessNotFound",
                    "The order could not determine its business."));
        }

        if (businessIds.Count > 1)
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.MultipleBusinesses",
                    "An order cannot contain products from multiple businesses."));
        }

        var businessId = businessIds[0];

        if (businessId == default ||
            products.Any(x => x.BusinessId != businessId))
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.BusinessNotFound",
                    "The order could not determine its business."));
        }

        var business = await _businessRepository.GetByIdAsync(
            businessId,
            cancellationToken);

        if (business is null)
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.BusinessNotFound",
                    "The business could not be found."));
        }

        if (business.Status != BusinessStatus.Active ||
            !business.IsApproved ||
            string.IsNullOrWhiteSpace(business.IyzicoSubMerchantKey) ||
            !string.Equals(
                business.IyzicoSubMerchantStatus,
                "SUCCESS",
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.BusinessNotAvailable",
                    "This business is not approved and ready to accept orders."));
        }

        // Calculate promotion pricing for each cart item
        var pricingByItem =
            new Dictionary<(ProductId ProductId, Guid? VariantId), PromotionPricingResult>();

        foreach (var cartItem in cart.Items)
        {
            var product = products.First(
                x => x.Id == cartItem.ProductId);

            var variant = cartItem.VariantId.HasValue
                ? product.Variants.FirstOrDefault(
                    x => x.Id == cartItem.VariantId.Value &&
                         x.IsActive)
                : null;

            Money originalUnitPrice = variant is not null
                ? Money.Create(variant.Price, product.Price.Currency)
                : product.Price;

            var pricing = await _promotionPricingService.CalculateAsync(
                businessId,
                product.Id,
                originalUnitPrice,
                cancellationToken);

            pricingByItem[(cartItem.ProductId, cartItem.VariantId)] = pricing;
        }

        // Calculate subtotal after discounts
        var subtotal = cart.Items.Sum(cartItem =>
        {
            var pricing = pricingByItem[
                (cartItem.ProductId, cartItem.VariantId)];

            return pricing.FinalUnitPrice.Amount *
                   cartItem.Quantity!.Value;
        });

        var shippingFee =
            business.FreeShippingThreshold.HasValue &&
            subtotal >= business.FreeShippingThreshold.Value
                ? 0m
                : business.ShippingFee;


        Result<CreateOrderResponse>? orderResponse = null;

        await _businessDbContext.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                // Load active branches inside the transaction
                var branches = await _businessDbContext.Branches
                    .Where(x =>
                        x.BusinessId == businessId &&
                        x.IsActive)
                    .ToListAsync(ct);

                if (branches.Count == 0)
                {
                    orderResponse =
                        Result<CreateOrderResponse>.Failure(
                            OrderErrors.ProductNotAvailable);

                    return;
                }

                var branchIds = branches
                    .Select(x => x.Id)
                    .ToList();

                // Load branch inventories inside the transaction
                var branchInventories =
                    await _businessDbContext.BranchInventories
                        .Where(x => branchIds.Contains(x.BranchId))
                        .ToListAsync(ct);

                // Aggregate repeated cart lines
                var stockRequirements = cart.Items
                    .GroupBy(x => new
                    {
                        x.ProductId,
                        x.VariantId
                    })
                    .Select(group => new
                    {
                        group.Key.ProductId,
                        group.Key.VariantId,
                        Quantity = group.Sum(
                            x => x.Quantity!.Value)
                    })
                    .ToList();

                bool HasRequiredStock(Branch branch)
                {
                    foreach (var requirement in stockRequirements)
                    {
                        var inventory = branchInventories.FirstOrDefault(
                            x =>
                                x.BranchId == branch.Id &&
                                x.ProductId == requirement.ProductId &&
                                x.VariantId == requirement.VariantId);

                        if (inventory is null ||
                            inventory.AvailableQuantity <
                            requirement.Quantity)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                var eligibleBranches = branches
                    .Where(HasRequiredStock)
                    .ToList();

                Branch? selectedBranch = null;

                if (deliveryAddress.Latitude.HasValue &&
                    deliveryAddress.Longitude.HasValue)
                {
                    double DistanceInKm(Branch branch)
                    {
                        const double earthRadiusKm = 6371;

                        var lat1 = (double)deliveryAddress.Latitude.Value *
                                   Math.PI / 180;

                        var lat2 = (double)branch.Latitude!.Value *
                                   Math.PI / 180;

                        var deltaLat =
                            lat2 - lat1;

                        var deltaLon =
                            ((double)branch.Longitude!.Value -
                             (double)deliveryAddress.Longitude.Value) *
                            Math.PI / 180;

                        var a =
                            Math.Pow(Math.Sin(deltaLat / 2), 2) +
                            Math.Cos(lat1) *
                            Math.Cos(lat2) *
                            Math.Pow(Math.Sin(deltaLon / 2), 2);

                        var c = 2 * Math.Atan2(
                            Math.Sqrt(a),
                            Math.Sqrt(1 - a));

                        return earthRadiusKm * c;
                    }

                    selectedBranch = eligibleBranches
                        .Where(x =>
                            x.Latitude.HasValue &&
                            x.Longitude.HasValue)
                        .OrderBy(DistanceInKm)
                        .FirstOrDefault();
                    selectedBranch ??= eligibleBranches
                        .FirstOrDefault(x => x.IsMain);
                }
                else
                {
                    // No customer coordinates: use the main branch.
                    selectedBranch = eligibleBranches
                        .FirstOrDefault(x => x.IsMain);
                }

                if (selectedBranch is null)
                {
                    orderResponse =
                        Result<CreateOrderResponse>.Failure(
                            OrderErrors.InsufficientStock);

                    return;
                }

                // Create order with selected branch
                var orderResult = Order.Create(
                    userId,
                    businessId,
                    selectedBranch.Id,
                    deliveryAddress,
                    shippingFee);

                if (orderResult.IsFailure)
                {
                    orderResponse =
                        Result<CreateOrderResponse>.Failure(
                            orderResult.Error);

                    return;
                }

                var order = orderResult.Value!;

                // Add order item snapshots
                foreach (var cartItem in cart.Items)
                {
                    var product = products.First(
                        x => x.Id == cartItem.ProductId);

                    var variant = cartItem.VariantId.HasValue
                        ? product.Variants.FirstOrDefault(
                            x => x.Id == cartItem.VariantId.Value &&
                                 x.IsActive)
                        : null;

                    if (cartItem.VariantId.HasValue &&
                        variant is null)
                    {
                        orderResponse =
                            Result<CreateOrderResponse>.Failure(
                                OrderErrors.ProductNotAvailable);

                        return;
                    }

                    var pricing = pricingByItem[
    (cartItem.ProductId, cartItem.VariantId)];

                    var variantName = variant?.Name;

                    var addItemResult = order.AddItem(
                        product.Id,
                        product.Name.Value,
                        pricing.OriginalUnitPrice,
                        pricing.DiscountAmount,
                        pricing.FinalUnitPrice,
                        cartItem.Quantity!.Value,
                        cartItem.VariantId,
                        variantName,
                        pricing.PromotionId,
                        pricing.PromotionName);

                    if (addItemResult.IsFailure)
                    {
                        orderResponse =
                            Result<CreateOrderResponse>.Failure(
                                addItemResult.Error);

                        return;
                    }
                }

                // Deduct stock from the selected branch
                foreach (var requirement in stockRequirements)
                {
                    var inventory = branchInventories.First(
                        x =>
                            x.BranchId == selectedBranch.Id &&
                            x.ProductId == requirement.ProductId &&
                            x.VariantId == requirement.VariantId);

                    inventory.DecreaseStock(requirement.Quantity);
                }

                // Add order and save everything using the same DbContext
                await _orderRepository.AddAsync(order, ct);

                await _unitOfWork.SaveChangesAsync(ct);

                orderResponse =
                    Result<CreateOrderResponse>.Success(
                        new CreateOrderResponse(
                            order.Id.Value,
                            order.TotalAmount,
                            order.ShippingFee,
                            order.Status.ToString(),
                            order.Items.Sum(x => x.Quantity)));
            },
            cancellationToken);

        return orderResponse ??
            Result<CreateOrderResponse>.Failure(
                OrderErrors.InsufficientStock);
    }

}

using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

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
    public CreateOrderCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IBusinessRepository businessRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _businessRepository = businessRepository;
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



        // 2. Cart must contain items
        if (!cart.Items.Any())
        {
            return Result<CreateOrderResponse>.Failure(
                OrderErrors.EmptyOrder);
        }

        // 3. Create delivery address snapshot
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

        // 4. Load products and validate the cart
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

            // Product must be active
            if (product.Status != ProductStatus.Active ||
                product.IsDeleted)
            {
                return Result<CreateOrderResponse>.Failure(
                    OrderErrors.ProductNotAvailable);
            }
            if (cartItem.VariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(
                    x => x.Id == cartItem.VariantId.Value && x.IsActive);

                if (variant is null)
                {
                    return Result<CreateOrderResponse>.Failure(
                        OrderErrors.ProductNotAvailable);
                }

                if (variant.StockQuantity < cartItem.Quantity.Value)
                {
                    return Result<CreateOrderResponse>.Failure(
                        OrderErrors.InsufficientStock);
                }
            }
            else if (product.StockQuantity < cartItem.Quantity.Value)
            {
                return Result<CreateOrderResponse>.Failure(
                    OrderErrors.InsufficientStock);
            }

            products.Add(product);
        }

        // 5. Order currently belongs to one Business
        var businessIds = products
            .Select(x => x.BusinessId)
            .Distinct()
            .ToList();
        foreach (var product in products)
        {
            Console.WriteLine(
                $"ORDER PRODUCT | ProductId={product.Id.Value} | BusinessId={product.BusinessId.Value}");
        }

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
        if (businessId == default)
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.BusinessNotFound",
                    "The order could not determine its business."));
        }

        if (products.Any(x => x.BusinessId != businessId))
        {
            return Result<CreateOrderResponse>.Failure(
                new Error(
                    "Order.MultipleBusinesses",
                    "An order cannot contain products from multiple businesses."));
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
        // 6. Create Order
        // 6. Calculate shipping
        var subtotal = cart.Items.Sum(cartItem =>
        {
            var product = products.First(
                x => x.Id == cartItem.ProductId);

            var variant = cartItem.VariantId.HasValue
    ? product.Variants.FirstOrDefault(
        x => x.Id == cartItem.VariantId.Value && x.IsActive)
    : null;

            var unitPrice = variant is not null
                ? variant.Price
                : product.Price.Amount;

            return unitPrice * cartItem.Quantity.Value;
        });


        var shippingFee =
            business.FreeShippingThreshold.HasValue &&
            subtotal >= business.FreeShippingThreshold.Value
                ? 0m
                : business.ShippingFee;

        // 7. Create Order
        var orderResult = Order.Create(
    userId,
    businessId,
    deliveryAddress,
    shippingFee);

        if (orderResult.IsFailure)
        {
            return Result<CreateOrderResponse>.Failure(
                orderResult.Error);
        }

        var order = orderResult.Value!;

        // 7. Decrease stock and add product snapshots
        for (var i = 0; i < cart.Items.Count; i++)
        {
            var cartItem = cart.Items.ElementAt(i);
            var product = products[i];

            var variant = cartItem.VariantId.HasValue
                ? product.Variants.FirstOrDefault(
                    x => x.Id == cartItem.VariantId.Value && x.IsActive)
                : null;

            if (cartItem.VariantId.HasValue && variant is null)
            {
                return Result<CreateOrderResponse>.Failure(
                    OrderErrors.ProductNotAvailable);
            }

            var unitPrice = product.Price;
            string? variantName = null;

            if (variant is not null)
            {
                if (variant.StockQuantity < cartItem.Quantity.Value)
                {
                    return Result<CreateOrderResponse>.Failure(
                        OrderErrors.InsufficientStock);
                }

                try
                {
                    variant.DecreaseStock(cartItem.Quantity.Value);
                }
                catch (InvalidOperationException)
                {
                    return Result<CreateOrderResponse>.Failure(
                        OrderErrors.InsufficientStock);
                }

                unitPrice = eMarket.Domain.Catalog.Products.ValueObjects.Money.Create(
                    variant.Price,
                    product.Price.Currency);

                variantName = variant.Name;
            }
            else
            {
                var stockResult = product.DecreaseStock(
                    cartItem.Quantity.Value);

                if (stockResult.IsFailure)
                {
                    return Result<CreateOrderResponse>.Failure(
                        stockResult.Error);
                }
            }

            var addItemResult = order.AddItem(
                product.Id,
                product.Name.Value,
                unitPrice,
                cartItem.Quantity.Value,
                cartItem.VariantId,
                variantName);

            if (addItemResult.IsFailure)
            {
                return Result<CreateOrderResponse>.Failure(
                    addItemResult.Error);
            }
        }


        // 9. Add Order
        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        // 10. Save everything
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        // 11. Response
        return Result<CreateOrderResponse>.Success(
           new CreateOrderResponse(
    order.Id.Value,
    order.TotalAmount,
    order.ShippingFee,
    order.Status.ToString(),
    order.Items.Sum(x => x.Quantity)));
    }

}

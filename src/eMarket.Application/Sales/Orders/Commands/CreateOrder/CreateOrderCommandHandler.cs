using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Identity;
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

    public CreateOrderCommandHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        // 1. Get active cart
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

        // 4. Create Order
        var orderResult = Order.Create(
            userId,
            deliveryAddress);

        if (orderResult.IsFailure)
        {
            return Result<CreateOrderResponse>.Failure(
                orderResult.Error);
        }

        var order = orderResult.Value!;

        // 5. Process cart items
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

            // Check stock
            if (product.StockQuantity < cartItem.Quantity.Value)
            {
                return Result<CreateOrderResponse>.Failure(
                    OrderErrors.InsufficientStock);
            }

            // Decrease product stock
            var stockResult = product.DecreaseStock(
                cartItem.Quantity.Value);

            if (stockResult.IsFailure)
            {
                return Result<CreateOrderResponse>.Failure(
                    stockResult.Error);
            }

            // Add product snapshot to Order
            var addItemResult = order.AddItem(
                product.Id,
                product.Name.Value,
                product.Price,
                cartItem.Quantity.Value);

            if (addItemResult.IsFailure)
            {
                return Result<CreateOrderResponse>.Failure(
                    addItemResult.Error);
            }
        }

        // 6. Checkout cart
        var checkoutResult = cart.Checkout();

        if (checkoutResult.IsFailure)
        {
            return Result<CreateOrderResponse>.Failure(
                checkoutResult.Error);
        }

        // 7. Add Order
        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        // 8. Save everything
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        // 9. Response
        return Result<CreateOrderResponse>.Success(
            new CreateOrderResponse(
                order.Id.Value,
                order.TotalAmount,
                order.Status.ToString(),
                order.Items.Sum(x => x.Quantity)));
    }
}

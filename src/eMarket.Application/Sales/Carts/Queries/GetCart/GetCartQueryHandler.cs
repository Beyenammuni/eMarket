using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Results;
using MediatR;
using eMarket.Domain.Sales.Carts.ValueObjects;

namespace eMarket.Application.Sales.Carts.Queries.GetCart;

internal sealed class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, Result<GetCartResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetCartResponse>> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<GetCartResponse>.Failure(
                CartErrors.Unauthorized);
        }

        var cart = await _cartRepository.GetActiveByUserAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            return Result<GetCartResponse>.Failure(
                CartErrors.EmptyCart);
        }

        var changed = false;

        foreach (var item in cart.Items.ToList())
        {
            var product = await _productRepository.GetByIdAsync(
                item.ProductId,
                cancellationToken);

            if (product is null ||
                product.Status.ToString() != "Active")
            {
                cart.RemoveItem(item.ProductId, item.VariantId);
                changed = true;
                continue;
            }

            var availableStock = product.StockQuantity;

            if (item.VariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(
                    x => x.Id == item.VariantId.Value &&
                         x.IsActive);

                if (variant is null)
                {
                    cart.RemoveItem(item.ProductId, item.VariantId);
                    changed = true;
                    continue;
                }

                availableStock = variant.StockQuantity;
            }

            if (availableStock <= 0)
            {
                cart.RemoveItem(item.ProductId, item.VariantId);
                changed = true;
                continue;
            }

            if (item.Quantity.Value > availableStock)
            {
                var quantityResult = cart.ChangeItemQuantity(
                    item.ProductId,
                    Quantity.Create(availableStock),
                    item.VariantId);

                if (quantityResult.IsSuccess)
                {
                    changed = true;
                }
            }
        }

        if (changed)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var items = new List<CartItemResponse>();

        foreach (var item in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                item.ProductId,
                cancellationToken);

            var variant = item.VariantId.HasValue
      ? product?.Variants.FirstOrDefault(
          x => x.Id == item.VariantId.Value)
      : null;

            var stockQuantity = variant?.StockQuantity
                ?? product?.StockQuantity
                ?? 0;

            var name = product?.Name.Value ?? "Ürün mevcut değil";

            if (variant is not null)
            {
                name = $"{name} - {variant.Name}";
            }

            items.Add(new CartItemResponse(
                item.ProductId.Value,
                item.VariantId,
                item.UnitPrice.Amount,
                item.UnitPrice.Currency,
                item.Quantity.Value,
                item.TotalPrice,
                stockQuantity,
                product?.Status.ToString() ?? "Unavailable",
                product?.BusinessId.Value ?? Guid.Empty,
                name,
                product?.ImageUrl));
        }

        var response = new GetCartResponse(
            cart.Id.Value,
            cart.TotalItems,
            cart.TotalAmount,
            items);

        return Result<GetCartResponse>.Success(response);
    }
}

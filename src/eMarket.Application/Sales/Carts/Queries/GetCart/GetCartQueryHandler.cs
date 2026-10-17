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
                product.Status.ToString() != "Active" ||
                product.StockQuantity <= 0)
            {
                cart.RemoveItem(item.ProductId);
                changed = true;
                continue;
            }

            if (item.Quantity.Value > product.StockQuantity)
            {
                var quantityResult = cart.ChangeItemQuantity(
                    item.ProductId,
                Quantity
                        .Create(product.StockQuantity));

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

            items.Add(new CartItemResponse(
     item.ProductId.Value,
     item.UnitPrice.Amount,
     item.UnitPrice.Currency,
     item.Quantity.Value,
     item.TotalPrice,
     product?.StockQuantity ?? 0,
     product?.Status.ToString() ?? "Unavailable",
     product?.BusinessId.Value ?? Guid.Empty,
     product?.Name.Value ?? "Ürün mevcut değil",
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

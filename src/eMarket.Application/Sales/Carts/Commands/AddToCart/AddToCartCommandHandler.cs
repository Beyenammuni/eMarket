using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Carts.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.AddToCart;

internal sealed class AddToCartCommandHandler
    : IRequestHandler<AddToCartCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AddToCartCommandHandler(
        IProductRepository productRepository,
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }


    public async Task<Result> Handle(
        AddToCartCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
            return Result.Failure(CartErrors.Unauthorized);

        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.ProductId),
            cancellationToken);

        if (product is null)
            return Result.Failure(ProductErrors.NotFound);

        if (product.Status != ProductStatus.Active || product.IsDeleted)
            return Result.Failure(ProductErrors.ProductNotAvailable);

        var unitPrice = product.Price;
        var availableStock = product.StockQuantity;
        Guid? variantId = null;
        string? variantName = null;

        if (request.VariantId.HasValue)
        {
            var variant = product.Variants.FirstOrDefault(
                x => x.Id == request.VariantId.Value);

            if (variant is null || !variant.IsActive)
                return Result.Failure(ProductErrors.NotFound);

            variantId = variant.Id;
            variantName = variant.Name;
            unitPrice = Money.Create(variant.Price, product.Price.Currency);
            availableStock = variant.StockQuantity;
        }
        Console.WriteLine($"ProductId: {request.ProductId}, VariantId: {request.VariantId}");

        var cart = await _cartRepository.GetActiveByUserAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            var cartResult = Cart.Create(userId);

            if (cartResult.IsFailure)
                return Result.Failure(cartResult.Error);

            cart = cartResult.Value!;

            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        var result = cart.AddItem(
            product.Id,
            variantId,
            variantName,
            unitPrice,
            Quantity.Create(request.Quantity),
            availableStock);

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Sales.Carts;
using eMarket.Domain.Sales.Carts.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.UpdateCartItem;

internal sealed class UpdateCartItemCommandHandler
    : IRequestHandler<UpdateCartItemCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateCartItemCommandHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        UpdateCartItemCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result.Failure(
                CartErrors.Unauthorized);
        }

        var cart = await _cartRepository.GetActiveByUserAsync(
            userId,
            cancellationToken);

        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.ProductId),
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                ProductErrors.NotFound);
        }

        if (product.Status != ProductStatus.Active)
        {
            return Result.Failure(
                ProductErrors.ProductNotAvailable);
        }
        var availableStock = product.StockQuantity;

        if (request.VariantId is not null)
        {
            var variant = product.Variants.FirstOrDefault(
                x => x.Id == request.VariantId.Value &&
                     x.IsActive);

            if (variant is null)
            {
                return Result.Failure(
                    ProductErrors.NotFound);
            }

            availableStock = variant.StockQuantity;
        }

        if (request.Quantity > availableStock)
        {
            return Result.Failure(
                CartErrors.InsufficientStock);
        }

        var result = cart.ChangeItemQuantity(
            product.Id,
            Quantity.Create(request.Quantity),
            request.VariantId);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

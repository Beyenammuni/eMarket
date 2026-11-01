
using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.Variants;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddProductVariant;

internal sealed class AddProductVariantHandler
    : IRequestHandler<AddProductVariantCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public AddProductVariantHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IBusinessAuthorization authorization)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<Result<Guid>> Handle(
        AddProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result<Guid>.Failure(
                ProductErrors.BusinessRequired);
        }

        var authorized =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Products.Update,
                cancellationToken);

        if (!authorized)
        {
            return Result<Guid>.Failure(
                ProductErrors.Forbidden);
        }

        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.ProductId),
            businessId,
            cancellationToken);

        if (product is null)
        {
            return Result<Guid>.Failure(
                ProductErrors.NotFound);
        }

        var variant = ProductVariant.Create(
            product.Id,
            request.Name,
            request.Sku,
            request.Price,
            request.StockQuantity);

        var addResult = product.AddVariant(variant);

        if (addResult.IsFailure)
        {
            return Result<Guid>.Failure(addResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(variant.Id);
    }
}

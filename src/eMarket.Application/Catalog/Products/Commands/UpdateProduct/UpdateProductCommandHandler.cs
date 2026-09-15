using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Catalog.Categories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler
    : IRequestHandler<
        UpdateProductCommand,
        Result<UpdateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public UpdateProductCommandHandler(
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

    public async Task<Result<UpdateProductResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result<UpdateProductResponse>.Failure(
                ProductErrors.BusinessRequired);
        }

        var authorized =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Products.Update,
                cancellationToken);

        if (!authorized)
        {
            return Result<UpdateProductResponse>.Failure(
                ProductErrors.Forbidden);
        }

        var product =
            await _productRepository.GetByIdAsync(
                ProductId.Create(request.Id),
                businessId,
                cancellationToken);

        if (product is null)
        {
            return Result<UpdateProductResponse>.Failure(
                ProductErrors.NotFound);
        }

        product.Rename(
            ProductName.Create(request.Name));

        product.ChangeDescription(
            ProductDescription.Create(request.Description));

        product.ChangePrice(
            Money.Create(request.Price, default));

        product.ChangeCategory(
            CategoryId.Create(request.CategoryId));

        product.ChangeSku(
            Sku.Create(request.Sku));

        product.ChangeImage(
            request.ImageUrl);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<UpdateProductResponse>.Success(
            new UpdateProductResponse(
                product.Id.Value,
                product.Name.Value,
                product.Description.Value,
                product.Price.Amount,
                product.Price.Currency,
                product.Sku.Value,
                product.CategoryId,
                product.ImageUrl.ToString()));
    }
}

using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;
using static eMarket.Application.Common.Authorization.Permissions;

namespace eMarket.Application.Catalog.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler
    : IRequestHandler<
        CreateProductCommand,
        Result<CreateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public CreateProductCommandHandler(
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

    public async Task<Result<CreateProductResponse>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.BusinessRequired);
        }

        var authorized =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Products.Create,
                cancellationToken);

        if (!authorized)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.Forbidden);
        }

        var categoryId =
            CategoryId.Create(request.CategoryId);

        var name =
            ProductName.Create(request.Name);

        var description =
            ProductDescription.Create(request.Description);

        var price =
            Money.Create(request.Price, default);

        var sku =
            Sku.Create(request.Sku);

        var exists = await _productRepository.ExistsAsync(
            businessId,
            name,
            null,
            cancellationToken);

        if (exists)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.AlreadyExists);
        }

        var productResult = Product.Create(
            businessId,
            categoryId,
            name,
            description,
            price,
            sku,
            request.ImageUrl);

        if (productResult.IsFailure)
        {
            return Result<CreateProductResponse>.Failure(
                productResult.Error);
        }

        var product = productResult.Value;

        await _productRepository.AddAsync(
            product,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<CreateProductResponse>.Success(
            new CreateProductResponse(
                product.Id.Value,
                product.Name.Value,
                product.Description.Value,
                product.Price.Amount,
                product.Price.Currency,
                product.Sku.Value,
                product.ImageUrl.ToString()
                ));
    }
}

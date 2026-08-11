using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler
    : IRequestHandler<
        UpdateProductCommand,
        Result<UpdateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateProductResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.Id),
            cancellationToken);

        if (product is null)
        {
            return Result<UpdateProductResponse>.Failure(
                ProductErrors.NotFound);
        }

        var category = await _categoryRepository.GetByIdAsync(
            CategoryId.Create(request.CategoryId),
            cancellationToken);

        if (category is null)
        {
            return Result<UpdateProductResponse>.Failure(
                ProductErrors.CategoryNotFound);
        }

        var name = ProductName.Create(request.Name);
        var description =
            ProductDescription.Create(request.Description);

        var price = Money.Create(
            request.Price,
            request.Currency);

        var sku = Sku.Create(request.Sku);

        var result = product.Rename(name);

        if (result.IsFailure)
        {
            return Result<UpdateProductResponse>.Failure(
                result.Error);
        }

        result = product.ChangeDescription(description);

        if (result.IsFailure)
        {
            return Result<UpdateProductResponse>.Failure(
                result.Error);
        }

        result = product.ChangePrice(price);

        if (result.IsFailure)
        {
            return Result<UpdateProductResponse>.Failure(
                result.Error);
        }

        result = product.ChangeSku(sku);

        if (result.IsFailure)
        {
            return Result<UpdateProductResponse>.Failure(
                result.Error);
        }

        result = product.ChangeCategory(
            CategoryId.Create(request.CategoryId));

        if (result.IsFailure)
        {
            return Result<UpdateProductResponse>.Failure(
                result.Error);
        }

        if (request.ImageUrl != product.ImageUrl)
        {
            result = product.ChangeImage(request.ImageUrl);

            if (result.IsFailure)
            {
                return Result<UpdateProductResponse>.Failure(
                    result.Error);
            }
        }

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
                product.CategoryId.Value,
                product.ImageUrl));
    }
}

using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler
    : IRequestHandler<
        CreateProductCommand,
        Result<CreateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateProductResponse>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check authentication
        if (!_currentUser.IsAuthenticated)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.Unauthorized);
        }

        // 2. Get current Business
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.BusinessRequired);
        }

        // 3. Check Category
        var category = await _categoryRepository.GetByIdAsync(
            CategoryId.Create(request.CategoryId),
            cancellationToken);

        if (category is null)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.CategoryNotFound);
        }

        // 4. Check duplicate Product
        var productName = ProductName.Create(request.Name);

        var exists = await _productRepository.ExistsAsync(
            businessId,
            productName,
            null,
            cancellationToken);

        if (exists)
        {
            return Result<CreateProductResponse>.Failure(
                ProductErrors.ProductAlreadyExists);
        }

        // 5. Create Product
        var result = Product.Create(
            businessId,
            CategoryId.Create(request.CategoryId),
            productName,
            ProductDescription.Create(request.Description),
            Money.Create(
                request.Price,
                request.Currency),
            Sku.Create(request.Sku),
            request.ImageUrl);

        if (result.IsFailure)
        {
            return Result<CreateProductResponse>.Failure(
                result.Error);
        }

        // 6. Add Product
        await _productRepository.AddAsync(
            result.Value,
            cancellationToken);

        // 7. Save changes
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        // 8. Return response
        return Result<CreateProductResponse>.Success(
    new CreateProductResponse(
        result.Value.Id.Value,
        result.Value.Name.Value,
        result.Value.Description.Value,
        result.Value.Price.Amount,
        result.Value.Price.Currency,
        result.Value.Sku.Value,
        result.Value.ImageUrl));
    }
}

using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateCategoryResponse>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.BusinessId is null)
        {
            return Result<CreateCategoryResponse>.Failure(
                CategoryErrors.BusinessRequired);
        }

        var businessId = _currentUser.BusinessId;

        var categoryName = CategoryName.Create(request.Name);

        var existingCategory = await _categoryRepository.GetByNameAsync(
            categoryName,
            businessId,
            cancellationToken);

        if (existingCategory is not null)
        {
            return Result<CreateCategoryResponse>.Failure(
                CategoryErrors.NameAlreadyExists);
        }

        CategoryId? parentCategoryId = request.ParentCategoryId.HasValue
            ? CategoryId.Create(request.ParentCategoryId.Value)
            : null;

        var categoryResult = Category.Create(
            businessId,
            categoryName,
            parentCategoryId);

        if (categoryResult.IsFailure)
        {
            return Result<CreateCategoryResponse>.Failure(
                categoryResult.Error);
        }

        await _categoryRepository.AddAsync(
            categoryResult.Value!,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var category = categoryResult.Value!;

        return Result<CreateCategoryResponse>.Success(
            new CreateCategoryResponse(
                category.Id.Value,
                category.Name.Value,
                category.Status.ToString(),
                category.CreatedAt));
    }
}

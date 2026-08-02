using eMarket.Application.Common.Interfaces;
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

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCategoryResponse>> Handle(
      CreateCategoryCommand request,
      CancellationToken cancellationToken)
    {
        var categoryName = CategoryName.Create(request.Name);

        var existingCategory = await _categoryRepository.GetByNameAsync(
            categoryName,
            cancellationToken);

        if (existingCategory is not null)
        {
            return Result<CreateCategoryResponse>.Failure(
                CategoryErrors.NameAlreadyExists);
        }

        var categoryResult = Category.Create(categoryName);

        if (categoryResult.IsFailure)
            return Result<CreateCategoryResponse>.Failure(categoryResult.Error);

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

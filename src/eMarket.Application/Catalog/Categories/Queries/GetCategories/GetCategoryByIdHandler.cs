using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using eMarket.Domain.Catalog.Categories;

namespace eMarket.Application.Catalog.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            CategoryId.Create(request.Id),
            cancellationToken);

        if (category is null)
            return Result<CategoryResponse>.Failure(CategoryErrors.NotFound);

        return Result<CategoryResponse>.Success(
            new CategoryResponse(
                category.Id.Value,
                category.Name.Value,
                category.Status.ToString(),
                category.CreatedAt));
    }
}

using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using eMarket.Domain.Catalog.Categories;

namespace eMarket.Application.Catalog.Categories.Queries.GetCategoryById;

public sealed class GetCategoriesHandler
    : IRequestHandler<GetCategories, Result<List<CategoryResponse>>>
{
    private readonly ICategoryRepository _category;

    public GetCategoriesHandler(ICategoryRepository categoryRepository)
    {
        _category = categoryRepository;
    }

    public async Task<Result<List<CategoryResponse>>> Handle(
           GetCategories request,
           CancellationToken cancellationToken)
    {
        var categories = await _category.GetAllAsync(cancellationToken);

        var response = categories
            .Select(category =>
                new CategoryResponse(
                    category.Id.Value,
                    category.Name.Value,
                    category.Status.ToString(),
                    category.CreatedAt))
            .ToList();

        return Result<List<CategoryResponse>>.Success(response);
    }
}

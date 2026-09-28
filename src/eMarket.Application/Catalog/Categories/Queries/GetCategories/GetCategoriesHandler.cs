using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using eMarket.Domain.Catalog.Categories;
using eMarket.Application.Common.IRepositories;

namespace eMarket.Application.Catalog.Categories.Queries.GetCategories;

public sealed class GetCategoriesHandler
    : IRequestHandler<GetCategories, Result<List<CategoryResponse>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUser _currentUser;

    public GetCategoriesHandler(
        ICategoryRepository categoryRepository,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CategoryResponse>>> Handle(
        GetCategories request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.BusinessId is null)
        {
            return Result<List<CategoryResponse>>.Failure(
                CategoryErrors.BusinessRequired);
        }

        var businessId = _currentUser.BusinessId;

        var categories = await _categoryRepository.GetAllAsync(
            businessId,
            cancellationToken);

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

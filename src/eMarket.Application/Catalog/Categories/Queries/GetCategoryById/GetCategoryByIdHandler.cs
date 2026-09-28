using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.SharedKernel.Results;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.Application.Common.IRepositories;

namespace eMarket.Application.Catalog.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUser _currentUser;

    public GetCategoryByIdHandler(
        ICategoryRepository categoryRepository,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<CategoryResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.BusinessId is null)
        {
            return Result<CategoryResponse>.Failure(
                CategoryErrors.BusinessRequired);
        }

        var businessId = _currentUser.BusinessId;

        var category = await _categoryRepository.GetByIdAsync(
            CategoryId.Create(request.Id),
            businessId,
            cancellationToken);

        if (category is null)
        {
            return Result<CategoryResponse>.Failure(
                CategoryErrors.NotFound);
        }

        return Result<CategoryResponse>.Success(
            new CategoryResponse(
                category.Id.Value,
                category.Name.Value,
                category.Status.ToString(),
                category.CreatedAt));
    }
}


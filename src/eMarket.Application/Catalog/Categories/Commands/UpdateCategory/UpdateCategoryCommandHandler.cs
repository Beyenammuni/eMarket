using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.SharedKernel.Results;
using eMarket.Application.Common.IRepositories;

namespace eMarket.Application.Catalog.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryResponse>>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateCategoryCommandHandler(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<UpdateCategoryResponse>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.BusinessId is null)
        {
            return Result<UpdateCategoryResponse>
                .Failure(CategoryErrors.BusinessRequired);
        }

        var businessId = _currentUser.BusinessId;

        var category = await _repository.GetByIdAsync(
            CategoryId.Create(request.Id),
            businessId,
            cancellationToken);

        if (category is null)
        {
            return Result<UpdateCategoryResponse>
                .Failure(CategoryErrors.NotFound);
        }

        var newName = CategoryName.Create(request.Name);

        var exists = await _repository.ExistsAsync(
            newName,
            businessId,
            category.Id,
            cancellationToken);

        if (exists)
        {
            return Result<UpdateCategoryResponse>
                .Failure(CategoryErrors.NameAlreadyExists);
        }

        var renameResult = category.Rename(newName);

        if (renameResult.IsFailure)
        {
            return Result<UpdateCategoryResponse>
                .Failure(renameResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateCategoryResponse>.Success(
            new UpdateCategoryResponse(
                category.Id.Value,
                category.Name.Value,
                category.Status.ToString()));
    }
}

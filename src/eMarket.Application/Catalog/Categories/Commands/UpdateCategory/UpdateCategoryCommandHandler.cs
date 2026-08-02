using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Catalog.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryResponse>>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateCategoryResponse>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(
            CategoryId.Create(request.Id),
            cancellationToken);

        if (category is null)
            return Result<UpdateCategoryResponse>
                .Failure(CategoryErrors.NotFound);

        var newName = CategoryName.Create(request.Name);
        var exists = await _repository.ExistsAsync(
            newName,
            category.Id,
            cancellationToken);

        if (exists)
        {
            return Result<UpdateCategoryResponse>
                .Failure(CategoryErrors.NameAlreadyExists);
        }

        var renameResult = category.Rename(newName);

        if (renameResult.IsFailure)
            return Result<UpdateCategoryResponse>
                .Failure(renameResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateCategoryResponse>.Success(
            new UpdateCategoryResponse(
                category.Id.Value,
                category.Name.Value,
                category.Status.ToString()));
    }
}

using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Catalog.Categories;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Catalog.Categories.Commands.DeactivateCategory;

public sealed class DeactivateCategoryHandler
    : IRequestHandler<DeactivateCategoryCommand, Result>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCategoryHandler(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeactivateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(
            CategoryId.Create(request.Id),
            cancellationToken);

        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        var result = category.Deactivate();

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using MediatR;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.SharedKernel.Results;
using eMarket.Application.Common.IRepositories;

namespace eMarket.Application.Catalog.Categories.Commands.DeactiveCategory;

public sealed class DeactivateCategoryHandler
    : IRequestHandler<DeactivateCategoryCommand, Result>
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeactivateCategoryHandler(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeactivateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.BusinessId is null)
        {
            return Result.Failure(
                CategoryErrors.BusinessRequired);
        }

        var businessId = _currentUser.BusinessId;

        var category = await _repository.GetByIdAsync(
            CategoryId.Create(request.Id),
            businessId,
            cancellationToken);

        if (category is null)
        {
            return Result.Failure(
                CategoryErrors.NotFound);
        }

        var result = category.Deactivate();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}


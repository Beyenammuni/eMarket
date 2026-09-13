using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddStock;

internal sealed class AddStockCommandHandler
    : IRequestHandler<AddStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public AddStockCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IBusinessAuthorization authorization)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<Result> Handle(
        AddStockCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result.Failure(
                ProductErrors.BusinessRequired);
        }

        var authorized =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Products.ManageStock,
                cancellationToken);

        if (!authorized)
        {
            return Result.Failure(
                ProductErrors.Forbidden);
        }

        var product =
            await _productRepository.GetByIdAsync(
                ProductId.Create(request.Id),
                businessId,
                cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                ProductErrors.NotFound);
        }

        var result =
            product.IncreaseStock(request.Quantity);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

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

    public AddStockCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        AddStockCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check authentication
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(
                ProductErrors.Unauthorized);
        }

        // 2. Get current Business
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result.Failure(
                ProductErrors.BusinessRequired);
        }

        // 3. Get product owned by current Business
        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.Id),
            businessId,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                ProductErrors.NotFound);
        }

        // 4. Add stock
        var result = product.IncreaseStock(
            request.Quantity);

        if (result.IsFailure)
        {
            return result;
        }

        // 5. Save changes
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

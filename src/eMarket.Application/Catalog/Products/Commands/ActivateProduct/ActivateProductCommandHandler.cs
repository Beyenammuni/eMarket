using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.ActivateProduct;

internal sealed class ActivateProductCommandHandler
    : IRequestHandler<ActivateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public ActivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        ActivateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Check authentication
        if (!_currentUser.IsAuthenticated)
        {
            return Result.Failure(
                ProductErrors.Unauthorized);
        }

        // Get current Business
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result.Failure(
                ProductErrors.BusinessRequired);
        }

        // Get product owned by current Business
        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.Id),
            businessId,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                ProductErrors.NotFound);
        }

        // Activate
        var result = product.Activate();

        if (result.IsFailure)
        {
            return result;
        }

        // Save
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

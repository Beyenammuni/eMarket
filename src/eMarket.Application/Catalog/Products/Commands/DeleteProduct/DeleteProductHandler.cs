using eMarket.Application.Catalog.Products.Commands.DeleteProduct;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.DeleteProduct;
internal sealed class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure(
                ProductErrors.Unauthorized);

        if (_currentUser.BusinessId is null)
            return Result.Failure(
                ProductErrors.BusinessRequired);

        var product =
            await _productRepository.GetByIdAsync(
                ProductId.Create(request.Id),
                _currentUser.BusinessId,
                cancellationToken);

        if (product is null)
            return Result.Failure(
                ProductErrors.NotFound);

        var result = product.Delete();

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

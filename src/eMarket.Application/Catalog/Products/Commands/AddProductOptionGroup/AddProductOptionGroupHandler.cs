
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.Options;
using eMarket.SharedKernel.Constants;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddProductOptionGroup;

internal sealed class AddProductOptionGroupHandler
    : IRequestHandler<AddProductOptionGroupCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public AddProductOptionGroupHandler(
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

    public async Task<Result<Guid>> Handle(
        AddProductOptionGroupCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
        {
            return Result<Guid>.Failure(ProductErrors.BusinessRequired);
        }

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            Permissions.Products.Update,
            cancellationToken);

        if (!authorized)
        {
            return Result<Guid>.Failure(ProductErrors.Forbidden);
        }

        var product = await _productRepository.GetByIdAsync(
            ProductId.Create(request.ProductId),
            businessId,
            cancellationToken);

        if (product is null)
        {
            return Result<Guid>.Failure(ProductErrors.NotFound);
        }

        var optionGroup = ProductOptionGroup.Create(
            product.Id,
            request.Name,
            request.IsRequired,
            request.MinSelections,
            request.MaxSelections,
            request.DisplayOrder);

        var addResult = product.AddOptionGroup(optionGroup);

        if (addResult.IsFailure)
        {
            return Result<Guid>.Failure(addResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(optionGroup.Id);
    }
}

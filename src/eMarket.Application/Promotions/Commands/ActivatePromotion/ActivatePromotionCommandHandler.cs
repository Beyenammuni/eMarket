
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Promotions;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;

namespace eMarket.Application.Promotions.Commands.ActivatePromotion;

internal sealed class ActivatePromotionCommandHandler
    : IRequestHandler<
        ActivatePromotionCommand,
        Result<ActivatePromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public ActivatePromotionCommandHandler(
        IPromotionRepository promotionRepository,
        IProductRepository productRepository,
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IBusinessAuthorization authorization)
    {
        _promotionRepository = promotionRepository;
        _productRepository = productRepository;
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<Result<ActivatePromotionResponse>> Handle(
        ActivatePromotionCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<ActivatePromotionResponse>.Failure(
                PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.Activate,
            cancellationToken);

        if (!authorized)
            return Result<ActivatePromotionResponse>.Failure(
                PromotionErrors.Forbidden);

        var promotionId = PromotionId.Create(request.PromotionId);

        Error? operationError = null;
        ActivatePromotionResponse? response = null;

        await _unitOfWork.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                // 1. Get promotion belonging to the current business.
                var promotion = await _promotionRepository.GetByIdAsync(
                    promotionId,
                    businessId,
                    ct);

                if (promotion is null)
                {
                    operationError = PromotionErrors.PromotionNotFound;
                    return;
                }

                // Only paused or scheduled campaigns can be reactivated.
                if (promotion.Status != PromotionStatus.Paused &&
                    promotion.Status != PromotionStatus.Scheduled)
                {
                    operationError = PromotionErrors.InvalidPromotion;
                    return;
                }

                // 2. Ensure the business is active.
                var business = await _businessRepository.GetByIdAsync(
                    businessId,
                    ct);

                if (business is null ||
                    business.Status != BusinessStatus.Active)
                {
                    operationError = PromotionErrors.BusinessNotActive;
                    return;
                }

                if (promotion.Items.Count == 0)
                {
                    operationError = PromotionErrors.ProductsRequired;
                    return;
                }

                // 3. Load products belonging to this business.
                var productIds = promotion.Items
                    .Select(x => x.ProductId)
                    .ToArray();

                var products = await _productRepository.GetByIdsAsync(
                    productIds,
                    businessId,
                    ct);

                if (products.Count != productIds.Length)
                {
                    operationError = PromotionErrors.ProductsNotFound;
                    return;
                }

                if (products.Any(x => x.Status != ProductStatus.Active))
                {
                    operationError = PromotionErrors.ProductNotActive;
                    return;
                }

                // 4. Validate discounts against current product prices.
                try
                {
                    foreach (var item in promotion.Items)
                    {
                        var product = products.First(
                            x => x.Id == item.ProductId);

                        item.CalculateDiscountedPrice(product.Price);
                    }
                }
                catch (ArgumentException)
                {
                    operationError = PromotionErrors.InvalidPromotion;
                    return;
                }
                catch (InvalidOperationException)
                {
                    operationError = PromotionErrors.InvalidPromotion;
                    return;
                }

                // 5. Check overlapping campaigns.
                var hasOverlap =
                    await _promotionRepository.HasOverlappingPromotionAsync(
                        businessId,
                        productIds,
                        promotion.StartsAtUtc,
                        promotion.EndsAtUtc,
                        promotion.Id,
                        ct);

                if (hasOverlap)
                {
                    operationError = PromotionErrors.OverlappingPromotion;
                    return;
                }

                // 6. Reactivate the campaign.
                try
                {
                    promotion.Activate();
                }
                catch (InvalidOperationException)
                {
                    operationError = PromotionErrors.InvalidPromotion;
                    return;
                }

                await _unitOfWork.SaveChangesAsync(ct);

                response = new ActivatePromotionResponse(
                    promotion.Id.Value,
                    promotion.Status.ToString());
            },
            cancellationToken);

        if (operationError is not null)
            return Result<ActivatePromotionResponse>.Failure(operationError);

        return Result<ActivatePromotionResponse>.Success(response!);
    }
}

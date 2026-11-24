
using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.Enums;
using eMarket.SharedKernel.Results;
using MediatR;

using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;

namespace eMarket.Application.Promotions.Commands.CreatePromotion;

internal sealed class CreatePromotionCommandHandler
    : IRequestHandler<CreatePromotionCommand, Result<CreatePromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public CreatePromotionCommandHandler(
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

    public async Task<Result<CreatePromotionResponse>> Handle(
        CreatePromotionCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<CreatePromotionResponse>.Failure(
                PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.Create,
            cancellationToken);

        if (!authorized)
            return Result<CreatePromotionResponse>.Failure(
                PromotionErrors.Forbidden);

        if (request.Items is null || request.Items.Count == 0)
            return Result<CreatePromotionResponse>.Failure(
                PromotionErrors.ProductsRequired);

        if (request.StartsAtUtc.Kind != DateTimeKind.Utc ||
            request.EndsAtUtc.Kind != DateTimeKind.Utc ||
            request.EndsAtUtc <= request.StartsAtUtc ||
            request.EndsAtUtc <= DateTime.UtcNow)
        {
            return Result<CreatePromotionResponse>.Failure(
                PromotionErrors.InvalidPromotion);
        }

        var productIds = request.Items
            .Select(x => x.ProductId)
            .ToArray();

        if (productIds.Distinct().Count() != productIds.Length)
            return Result<CreatePromotionResponse>.Failure(
                PromotionErrors.InvalidPromotion);

        var typedProductIds = productIds
            .Select(ProductId.Create)
            .ToArray();

        Error? operationError = null;
        CreatePromotionResponse? response = null;

        await _unitOfWork.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                // 1. Ensure the business is active.
                var business = await _businessRepository.GetByIdAsync(
                    businessId, ct);

                if (business is null ||
                    business.Status != BusinessStatus.Active)
                {
                    operationError = PromotionErrors.BusinessNotActive;
                    return;
                }

                // 2. Load only products belonging to this business.
                var products = await _productRepository.GetByIdsAsync(
                    typedProductIds,
                    businessId,
                    ct);

                if (products.Count != typedProductIds.Length)
                {
                    operationError = PromotionErrors.ProductsNotFound;
                    return;
                }

                if (products.Any(p => p.Status != ProductStatus.Active))
                {
                    operationError = PromotionErrors.ProductNotActive;
                    return;
                }

                // 3. Create the promotion.
                Promotion promotion;

                try
                {
                    promotion = Promotion.Create(
                        businessId,
                        request.Name,
                        request.Description,
                        request.StartsAtUtc,
                        request.EndsAtUtc);

                    foreach (var itemRequest in request.Items)
                    {
                        var productId = ProductId.Create(
                            itemRequest.ProductId);

                        var product = products.First(
                            p => p.Id == productId);

                        var item = PromotionItem.Create(
                            promotion.Id,
                            productId,
                            itemRequest.DiscountType,
                            itemRequest.DiscountValue);

                        // Validate the discount against the current price.
                        item.CalculateDiscountedPrice(product.Price);

                        promotion.AddItem(item);
                    }

                    // New campaigns are activated immediately:
                    // Active now, or Scheduled for a future start date.
                    promotion.Activate();
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

                // 4. Prevent overlapping campaigns for the same products.
                var hasOverlap =
                    await _promotionRepository.HasOverlappingPromotionAsync(
                        businessId,
                        typedProductIds,
                        request.StartsAtUtc,
                        request.EndsAtUtc,
                        cancellationToken: ct);

                if (hasOverlap)
                {
                    operationError = PromotionErrors.OverlappingPromotion;
                    return;
                }

                // 5. Save inside the serializable transaction.
                await _promotionRepository.AddAsync(promotion, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                response = new CreatePromotionResponse(
                    promotion.Id.Value,
                    promotion.Name,
                    promotion.Status.ToString(),
                    promotion.StartsAtUtc,
                    promotion.EndsAtUtc,
                    promotion.Items.Select(item =>
                        new CreatePromotionItemResponse(
                            item.ProductId.Value,
                            item.DiscountType,
                            item.DiscountValue))
                        .ToList());
            },
            cancellationToken);

        if (operationError is not null)
            return Result<CreatePromotionResponse>.Failure(operationError);

        return Result<CreatePromotionResponse>.Success(response!);
    }
}

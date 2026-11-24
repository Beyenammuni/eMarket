
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Promotions;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;

namespace eMarket.Application.Promotions.Commands.UpdatePromotion;

internal sealed class UpdatePromotionCommandHandler
    : IRequestHandler<
        UpdatePromotionCommand,
        Result<UpdatePromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public UpdatePromotionCommandHandler(
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

    public async Task<Result<UpdatePromotionResponse>> Handle(
        UpdatePromotionCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<UpdatePromotionResponse>.Failure(
                PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.Update,
            cancellationToken);

        if (!authorized)
            return Result<UpdatePromotionResponse>.Failure(
                PromotionErrors.Forbidden);

        if (request.Items is null || request.Items.Count == 0)
            return Result<UpdatePromotionResponse>.Failure(
                PromotionErrors.ProductsRequired);

        if (request.StartsAtUtc.Kind != DateTimeKind.Utc ||
            request.EndsAtUtc.Kind != DateTimeKind.Utc ||
            request.EndsAtUtc <= request.StartsAtUtc ||
            request.EndsAtUtc <= DateTime.UtcNow)
        {
            return Result<UpdatePromotionResponse>.Failure(
                PromotionErrors.InvalidPromotion);
        }

        var productIds = request.Items
            .Select(x => x.ProductId)
            .ToArray();

        if (productIds.Distinct().Count() != productIds.Length)
            return Result<UpdatePromotionResponse>.Failure(
                PromotionErrors.InvalidPromotion);

        var typedProductIds = productIds
            .Select(ProductId.Create)
            .ToArray();

        Error? operationError = null;
        UpdatePromotionResponse? response = null;

        await _unitOfWork.ExecuteInSerializableTransactionAsync(
            async ct =>
            {
                var promotion = await _promotionRepository.GetByIdAsync(
                    PromotionId.Create(request.PromotionId),
                    businessId,
                    ct);

                if (promotion is null)
                {
                    operationError = PromotionErrors.PromotionNotFound;
                    return;
                }

                var business = await _businessRepository.GetByIdAsync(
                    businessId,
                    ct);

                if (business is null ||
                    business.Status != BusinessStatus.Active)
                {
                    operationError = PromotionErrors.BusinessNotActive;
                    return;
                }

                var products = await _productRepository.GetByIdsAsync(
                    typedProductIds,
                    businessId,
                    ct);

                if (products.Count != typedProductIds.Length)
                {
                    operationError = PromotionErrors.ProductsNotFound;
                    return;
                }

                if (products.Any(x => x.Status != ProductStatus.Active))
                {
                    operationError = PromotionErrors.ProductNotActive;
                    return;
                }

                try
                {
                    var newItems = new List<PromotionItem>();

                    foreach (var itemRequest in request.Items)
                    {
                        var productId = ProductId.Create(
                            itemRequest.ProductId);

                        var product = products.First(
                            x => x.Id == productId);

                        var item = PromotionItem.Create(
                            promotion.Id,
                            productId,
                            itemRequest.DiscountType,
                            itemRequest.DiscountValue);

                        // Validate discount against the current product price.
                        item.CalculateDiscountedPrice(product.Price);

                        newItems.Add(item);
                    }

                    // Check overlapping promotions before modifying the aggregate.
                    var hasOverlap =
                        await _promotionRepository.HasOverlappingPromotionAsync(
                            businessId,
                            typedProductIds,
                            request.StartsAtUtc,
                            request.EndsAtUtc,
                            promotion.Id,
                            ct);

                    if (hasOverlap)
                    {
                        operationError =
                            PromotionErrors.OverlappingPromotion;
                        return;
                    }

                    promotion.UpdateDetails(
                        request.Name,
                        request.Description,
                        request.StartsAtUtc,
                        request.EndsAtUtc);

                    promotion.ReplaceItems(newItems);
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

                await _unitOfWork.SaveChangesAsync(ct);

                response = new UpdatePromotionResponse(
                    promotion.Id.Value,
                    promotion.Name,
                    promotion.Status.ToString(),
                    promotion.StartsAtUtc,
                    promotion.EndsAtUtc,
                    promotion.Items.Select(item =>
                        new UpdatePromotionItemResponse(
                            item.ProductId.Value,
                            item.DiscountType,
                            item.DiscountValue))
                        .ToList());
            },
            cancellationToken);

        if (operationError is not null)
            return Result<UpdatePromotionResponse>.Failure(operationError);

        return Result<UpdatePromotionResponse>.Success(response!);
    }
}

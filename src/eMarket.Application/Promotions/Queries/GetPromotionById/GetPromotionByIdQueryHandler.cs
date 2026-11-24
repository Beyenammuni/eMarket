
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Promotions.Queries.GetPromotions;
using eMarket.Domain.Promotions;
using eMarket.SharedKernel.Results;
using MediatR;
using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;

namespace eMarket.Application.Promotions.Queries.GetPromotionById;

internal sealed class GetPromotionByIdQueryHandler
    : IRequestHandler<GetPromotionByIdQuery, Result<GetPromotionResponse>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public GetPromotionByIdQueryHandler(
        IPromotionRepository promotionRepository,
        IProductRepository productRepository,
        ICurrentUser currentUser,
        IBusinessAuthorization authorization)
    {
        _promotionRepository = promotionRepository;
        _productRepository = productRepository;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<Result<GetPromotionResponse>> Handle(
        GetPromotionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<GetPromotionResponse>.Failure(
                PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.View,
            cancellationToken);

        if (!authorized)
            return Result<GetPromotionResponse>.Failure(
                PromotionErrors.Forbidden);

        var promotion = await _promotionRepository.GetByIdAsync(
            eMarket.Domain.Promotions.ValueObjects.PromotionId.Create(
                request.PromotionId),
            businessId,
            cancellationToken);

        if (promotion is null)
            return Result<GetPromotionResponse>.Failure(
                PromotionErrors.PromotionNotFound);

        var productIds = promotion.Items
            .Select(item => item.ProductId)
            .Distinct()
            .ToArray();

        var products = await _productRepository.GetByIdsAsync(
            productIds,
            businessId,
            cancellationToken);

        var productLookup = products.ToDictionary(p => p.Id);
        var items = new List<GetPromotionItemResponse>();

        foreach (var item in promotion.Items)
        {
            if (!productLookup.TryGetValue(item.ProductId, out var product))
                continue;

            try
            {
                var discountedPrice =
                    item.CalculateDiscountedPrice(product.Price);

                items.Add(new GetPromotionItemResponse(
                    product.Id.Value,
                    product.Name.Value,
                    product.Price.Amount,
                    discountedPrice.Amount,
                    discountedPrice.Currency.ToString(),
                    item.DiscountType,
                    item.DiscountValue));
            }
            catch (InvalidOperationException)
            {
                // Skip products whose current price cannot support the discount.
            }
        }

        var response = new GetPromotionResponse(
            promotion.Id.Value,
            promotion.Name,
            promotion.Description,
            promotion.Status.ToString(),
            DateTime.SpecifyKind(promotion.StartsAtUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(promotion.EndsAtUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(promotion.CreatedAt, DateTimeKind.Utc),
            items);

        return Result<GetPromotionResponse>.Success(response);
    }
}

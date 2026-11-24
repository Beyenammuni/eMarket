
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Promotions;
using eMarket.SharedKernel.Results;
using MediatR;
using PromotionPermissions =
    eMarket.Application.Common.Authorization.Permissions.Promotions;
namespace eMarket.Application.Promotions.Queries.GetPromotions;

internal sealed class GetPromotionsQueryHandler
    : IRequestHandler<
        GetPromotionsQuery,
        Result<IReadOnlyCollection<GetPromotionResponse>>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public GetPromotionsQueryHandler(
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

    public async Task<Result<IReadOnlyCollection<GetPromotionResponse>>> Handle(
        GetPromotionsQuery request,
        CancellationToken cancellationToken)
    {
        var businessId = _currentUser.BusinessId;

        if (businessId is null)
            return Result<IReadOnlyCollection<GetPromotionResponse>>
                .Failure(PromotionErrors.BusinessRequired);

        var authorized = await _authorization.HasPermissionAsync(
            businessId,
            PromotionPermissions.View,
            cancellationToken);

        if (!authorized)
            return Result<IReadOnlyCollection<GetPromotionResponse>>
                .Failure(PromotionErrors.Forbidden);

        var promotions = await _promotionRepository.GetByBusinessIdAsync(
            businessId,
            cancellationToken);

        if (promotions.Count == 0)
        {
            return Result<IReadOnlyCollection<GetPromotionResponse>>
                .Success(Array.Empty<GetPromotionResponse>());
        }

        var productIds = promotions
            .SelectMany(p => p.Items)
            .Select(i => i.ProductId)
            .Distinct()
            .ToArray();

        var products = await _productRepository.GetByIdsAsync(
            productIds,
            businessId,
            cancellationToken);

        var productLookup = products.ToDictionary(p => p.Id);

        var response = promotions.Select(p =>
            new GetPromotionResponse(
                p.Id.Value,
                p.Name,
                p.Description,
                p.Status.ToString(),
               DateTime.SpecifyKind(p.StartsAtUtc, DateTimeKind.Utc),
DateTime.SpecifyKind(p.EndsAtUtc, DateTimeKind.Utc),
DateTime.SpecifyKind(p.CreatedAt, DateTimeKind.Utc),
                p.Items
                    .Where(item => productLookup.ContainsKey(item.ProductId))
                    .Select(item =>
                    {
                        var product = productLookup[item.ProductId];

                        var discountedPrice =
                            item.CalculateDiscountedPrice(product.Price);

                        return new GetPromotionItemResponse(
                            product.Id.Value,
                            product.Name.Value,
                            product.Price.Amount,
                            discountedPrice.Amount,
                            discountedPrice.Currency.ToString(),
                            item.DiscountType,
                            item.DiscountValue);
                    })
                    .ToList()))
            .ToArray();

        return Result<IReadOnlyCollection<GetPromotionResponse>>
            .Success(response);
    }
}

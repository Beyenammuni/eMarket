
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Promotions.Queries.GetActivePromotions;

internal sealed class GetActivePromotionsQueryHandler
    : IRequestHandler<
        GetActivePromotionsQuery,
        Result<IReadOnlyCollection<GetActivePromotionResponse>>>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IProductRepository _productRepository;

    public GetActivePromotionsQueryHandler(
        IPromotionRepository promotionRepository,
        IProductRepository productRepository)
    {
        _promotionRepository = promotionRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyCollection<GetActivePromotionResponse>>> Handle(
        GetActivePromotionsQuery request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var promotions = await _promotionRepository
            .GetActiveForCustomersAsync(
                nowUtc,
                cancellationToken);

        if (promotions.Count == 0)
        {
            return Result<IReadOnlyCollection<GetActivePromotionResponse>>
                .Success(Array.Empty<GetActivePromotionResponse>());
        }

        var response = new List<GetActivePromotionResponse>();

        foreach (var promotion in promotions)
        {
            var productIds = promotion.Items
                .Select(item => item.ProductId)
                .Distinct()
                .ToArray();

            var products = await _productRepository.GetByIdsAsync(
                productIds,
                promotion.BusinessId,
                cancellationToken);

            var productLookup = products
                .Where(product =>
                    product.Status == ProductStatus.Active &&
                    !product.IsDeleted)
                .ToDictionary(product => product.Id);

            var items = new List<ActivePromotionProductResponse>();

            foreach (var item in promotion.Items)
            {
                if (!productLookup.TryGetValue(
                    item.ProductId,
                    out var product))
                {
                    continue;
                }

                try
                {
                    var discountedPrice =
                        item.CalculateDiscountedPrice(product.Price);

                    items.Add(new ActivePromotionProductResponse(
                        product.Id.Value,
                        promotion.BusinessId.Value,
                        product.Name.Value,
                        product.Price.Amount,
                        discountedPrice.Amount,
                        discountedPrice.Currency.ToString(),
                        product.ImageUrl,
                        (int)item.DiscountType,
                        item.DiscountValue));
                }
                catch (InvalidOperationException)
                {
                    // Skip an item if its current price
                    // no longer supports the configured discount.
                }
            }

            if (items.Count == 0)
                continue;

            response.Add(new GetActivePromotionResponse(
                promotion.Id.Value,
                promotion.Name,
                DateTime.SpecifyKind(
                    promotion.StartsAtUtc, DateTimeKind.Utc),
                DateTime.SpecifyKind(
                    promotion.EndsAtUtc, DateTimeKind.Utc),
                items));
        }

        return Result<IReadOnlyCollection<GetActivePromotionResponse>>
            .Success(response);
    }
}

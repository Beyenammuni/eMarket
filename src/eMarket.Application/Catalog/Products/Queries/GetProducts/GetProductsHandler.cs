
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.ValueObjects;
using eMarket.Domain.Common;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Queries.GetProducts;

internal sealed class GetProductsHandler
    : IRequestHandler<GetProductsQuery, Result<GetProductsResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IPromotionRepository _promotionRepository;

    public GetProductsHandler(
        IProductRepository repository,
        IPromotionRepository promotionRepository)
    {
        _repository = repository;
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<GetProductsResponse>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (products, totalCount) =
            await _repository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.BusinessId is null
                    ? null
                    : BusinessId.Create(request.BusinessId.Value),
                request.CategoryId is null
                    ? null
                    : CategoryId.Create(request.CategoryId.Value),
                request.SortBy,
                request.SortDescending,
                cancellationToken);

        var promotionByProductId =
            new Dictionary<Guid, (Promotion Promotion, PromotionItem Item)>();

        var nowUtc = DateTime.UtcNow;

        // Fetch active promotions once per business.
        foreach (var businessGroup in products.GroupBy(
            product => product.BusinessId))
        {
            var productIds = businessGroup
                .Select(product => product.Id)
                .ToList();

            var productIdValues = productIds
                .Select(id => id.Value)
                .ToHashSet();

            var promotions =
                await _promotionRepository.GetActiveForProductsAsync(
                    businessGroup.Key,
                    productIds,
                    nowUtc,
                    cancellationToken);

            // Repository orders promotions by expiration date.
            // The first applicable promotion is selected per product.
            foreach (var promotion in promotions)
            {
                foreach (var item in promotion.Items)
                {
                    var productId = item.ProductId.Value;

                    if (!productIdValues.Contains(productId))
                        continue;

                    promotionByProductId.TryAdd(
                        productId,
                        (promotion, item));
                }
            }
        }

        var items = products
            .Select(product =>
            {
                PromotionPriceDto? productPromotion = null;

                PromotionPriceDto? variantPromotion = null;

                if (promotionByProductId.TryGetValue(
                    product.Id.Value,
                    out var promotionData))
                {
                    productPromotion = CreatePromotionPrice(
                        product.Price,
                        promotionData.Promotion,
                        promotionData.Item);
                }

                var variants = product.Variants
                    .Select(variant =>
                    {
                        if (promotionByProductId.TryGetValue(
                            product.Id.Value,
                            out var variantPromotionData))
                        {
                            var variantOriginalPrice = Money.Create(
                                variant.Price,
                                product.Price.Currency);

                            variantPromotion = CreatePromotionPrice(
                                variantOriginalPrice,
                                variantPromotionData.Promotion,
                                variantPromotionData.Item);
                        }
                        else
                        {
                            variantPromotion = null;
                        }

                        return new ProductVariantDto(
                            variant.Id,
                            variant.Name,
                            variant.Price,
                            variant.StockQuantity,
                            variant.IsActive,
                            variantPromotion);
                    })
                    .ToList();

                return new ProductDto(
                    product.Id.Value,
                    product.BusinessId.Value,
                    product.CategoryId.Value,
                    product.Name.Value,
                    product.Price.Amount,
                    product.ImageUrl,
                    product.StockQuantity,
                    product.Status.ToString(),
                    variants,
                    productPromotion);
            })
            .ToList();

        return Result<GetProductsResponse>.Success(
            new GetProductsResponse(
                items,
                request.Page,
                request.PageSize,
                totalCount));
    }

    private static PromotionPriceDto? CreatePromotionPrice(
        Money originalPrice,
        Promotion promotion,
        PromotionItem promotionItem)
    {
        try
        {
            var discountedPrice =
                promotionItem.CalculateDiscountedPrice(originalPrice);

            return new PromotionPriceDto(
                originalPrice.Amount,
                discountedPrice.Amount,
                promotionItem.DiscountType,
                promotionItem.DiscountValue,
                promotion.Name);
        }
        catch (InvalidOperationException)
        {
            // A fixed discount cannot be greater than or equal
            // to this particular product or variant price.
            return null;
        }
    }
}

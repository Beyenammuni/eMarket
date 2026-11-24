
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Catalog.Products.Queries.GetProducts;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Common;
using eMarket.Domain.Promotions.Entities;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Queries.GetProductById;

internal sealed class GetProductByIdHandler
    : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IPromotionRepository _promotionRepository;

    public GetProductByIdHandler(
        IProductRepository repository,
        IPromotionRepository promotionRepository)
    {
        _repository = repository;
        _promotionRepository = promotionRepository;
    }

    public async Task<Result<GetProductByIdResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            ProductId.Create(request.Id),
            cancellationToken);

        if (product is null)
        {
            return Result<GetProductByIdResponse>
                .Failure(ProductErrors.NotFound);
        }

        var promotions = await _promotionRepository.GetActiveForProductAsync(
            product.BusinessId,
            product.Id,
            DateTime.UtcNow,
            cancellationToken);

        var promotionData = promotions
            .SelectMany(promotion =>
                promotion.Items
                    .Where(item => item.ProductId.Value == product.Id.Value)
                    .Select(item => (Promotion: promotion, Item: item)))
            .FirstOrDefault();

        PromotionPriceDto? productPromotion = null;

        if (promotionData.Promotion is not null &&
            promotionData.Item is not null)
        {
            productPromotion = CreatePromotionPrice(
                product.Price,
                promotionData.Promotion,
                promotionData.Item);
        }

        var variants = product.Variants
            .Select(variant =>
            {
                PromotionPriceDto? variantPromotion = null;

                if (promotionData.Promotion is not null &&
                    promotionData.Item is not null)
                {
                    var originalPrice = Money.Create(
                        variant.Price,
                        product.Price.Currency);

                    variantPromotion = CreatePromotionPrice(
                        originalPrice,
                        promotionData.Promotion,
                        promotionData.Item);
                }

                return new ProductVariantResponse(
                    variant.Id,
                    variant.Name,
                    variant.Sku,
                    variant.Price,
                    variant.StockQuantity,
                    variant.IsActive,
                    variantPromotion);
            })
            .ToList();

        return Result<GetProductByIdResponse>.Success(
            new GetProductByIdResponse(
                product.Id.Value,
                product.BusinessId.Value,
                product.CategoryId.Value,
                product.Name.Value,
                product.Description.Value,
                product.Price.Amount,
                product.Sku.Value,
                product.ImageUrl,
                product.StockQuantity,
                product.Status.ToString(),
                product.CreatedAt,
                variants,
                product.OptionGroups
                    .Select(group => new ProductOptionGroupResponse(
                        group.Id,
                        group.Name,
                        group.IsRequired,
                        group.MinSelections,
                        group.MaxSelections,
                        group.DisplayOrder,
                        group.Options
                            .Select(option => new ProductOptionResponse(
                                option.Id,
                                option.Name,
                                option.AdditionalPrice,
                                option.IsActive,
                                option.DisplayOrder))
                            .ToList()))
                    .ToList(),
                productPromotion));
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
            // The fixed discount is not valid for this price.
            return null;
        }
    }
}

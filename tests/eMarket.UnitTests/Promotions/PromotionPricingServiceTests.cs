
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Services;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Common;
using eMarket.Domain.Promotions.Entities;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;
using FluentAssertions;
using Xunit;

namespace eMarket.UnitTests.Promotions;

public class PromotionPricingServiceTests
{
    [Fact]
    public async Task CalculateAsync_Should_Apply_Percentage_Discount_To_Variant_Price()
    {
        // Arrange
        var businessId = BusinessId.New();
        var productId = ProductId.New();

        var promotion = CreateActivePromotion(
            businessId,
            productId,
            DiscountType.Percentage,
            20m);

        var repository = new FakePromotionRepository(promotion);
        var service = new PromotionPricingService(repository);

        // Variant price is 200 TRY, not the base product price.
        var variantPrice = Money.Create(200m, Currency.TRY);

        // Act
        var result = await service.CalculateAsync(
            businessId,
            productId,
            variantPrice);

        // Assert
        result.OriginalUnitPrice.Amount.Should().Be(200m);
        result.DiscountAmount.Amount.Should().Be(40m);
        result.FinalUnitPrice.Amount.Should().Be(160m);
        result.PromotionId.Should().Be(promotion.Id.Value);
        result.PromotionName.Should().Be("Test Promotion");
    }

    [Fact]
    public async Task CalculateAsync_Should_Return_Original_Price_When_No_Promotion_Exists()
    {
        // Arrange
        var repository = new FakePromotionRepository();
        var service = new PromotionPricingService(repository);

        var price = Money.Create(200m, Currency.TRY);

        // Act
        var result = await service.CalculateAsync(
            BusinessId.New(),
            ProductId.New(),
            price);

        // Assert
        result.OriginalUnitPrice.Amount.Should().Be(200m);
        result.DiscountAmount.Amount.Should().Be(0m);
        result.FinalUnitPrice.Amount.Should().Be(200m);
        result.PromotionId.Should().BeNull();
        result.PromotionName.Should().BeNull();
    }

    private static Promotion CreateActivePromotion(
        BusinessId businessId,
        ProductId productId,
        DiscountType discountType,
        decimal discountValue)
    {
        var now = DateTime.UtcNow;

        var promotion = Promotion.Create(
            businessId,
            "Test Promotion",
            null,
            now.AddHours(-1),
            now.AddHours(1));

        var item = PromotionItem.Create(
            promotion.Id,
            productId,
            discountType,
            discountValue);

        promotion.AddItem(item);
        promotion.Activate();

        return promotion;
    }

    private sealed class FakePromotionRepository
        : IPromotionRepository
    {
        private readonly List<Promotion> _promotions;

        public FakePromotionRepository(
            params Promotion[] promotions)
        {
            _promotions = promotions.ToList();
        }

        public Task<List<Promotion>> GetActiveForProductAsync(
            BusinessId businessId,
            ProductId productId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var result = _promotions
                .Where(p => p.BusinessId == businessId)
                .Where(p => p.IsCurrentlyActive(nowUtc))
                .Where(p => p.Items.Any(
                    item => item.ProductId == productId))
                .ToList();

            return Task.FromResult(result);
        }

        public Task<Promotion?> GetByIdAsync(
            PromotionId id,
            BusinessId businessId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task AddAsync(
            Promotion promotion,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> HasOverlappingPromotionAsync(
            BusinessId businessId,
            IReadOnlyCollection<ProductId> productIds,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            PromotionId? excludedPromotionId = null,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<List<Promotion>> GetByBusinessIdAsync(
            BusinessId businessId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<List<Promotion>> GetActiveForCustomersAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<List<Promotion>> GetActiveForProductsAsync(BusinessId businessId, IReadOnlyCollection<ProductId> productIds, DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
    [Fact]
    public async Task CalculateAsync_Should_Apply_Fixed_Discount_To_Variant_Price()
    {
        // Arrange
        var businessId = BusinessId.New();
        var productId = ProductId.New();

        var promotion = CreateActivePromotion(
            businessId,
            productId,
            DiscountType.FixedAmount,
            30m);

        var repository = new FakePromotionRepository(promotion);
        var service = new PromotionPricingService(repository);

        var variantPrice = Money.Create(200m, Currency.TRY);

        // Act
        var result = await service.CalculateAsync(
            businessId,
            productId,
            variantPrice);

        // Assert
        result.OriginalUnitPrice.Amount.Should().Be(200m);
        result.DiscountAmount.Amount.Should().Be(30m);
        result.FinalUnitPrice.Amount.Should().Be(170m);
    }
    [Fact]
    public async Task CalculateAsync_Should_Not_Apply_Fixed_Discount_When_It_Exceeds_Variant_Price()
    {
        var businessId = BusinessId.New();
        var productId = ProductId.New();

        var promotion = CreateActivePromotion(
            businessId, productId, DiscountType.FixedAmount, 100m);

        var repository = new FakePromotionRepository(promotion);
        var service = new PromotionPricingService(repository);
        var variantPrice = Money.Create(80m, Currency.TRY);

        var result = await service.CalculateAsync(
            businessId, productId, variantPrice);

        result.OriginalUnitPrice.Amount.Should().Be(80m);
        result.DiscountAmount.Amount.Should().Be(0m);
        result.FinalUnitPrice.Amount.Should().Be(80m);
        result.PromotionId.Should().BeNull();
    }
    [Fact]
    public async Task CalculateAsync_Should_Throw_When_Multiple_Active_Promotions_Exist()
    {
        var businessId = BusinessId.New();
        var productId = ProductId.New();

        var firstPromotion = CreateActivePromotion(
            businessId, productId, DiscountType.Percentage, 10m);

        var secondPromotion = CreateActivePromotion(
            businessId, productId, DiscountType.Percentage, 20m);

        var repository = new FakePromotionRepository(
            firstPromotion,
            secondPromotion);

        var service = new PromotionPricingService(repository);

        var price = Money.Create(200m, Currency.TRY);

        var act = async () => await service.CalculateAsync(
            businessId, productId, price);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Multiple active promotions*");
    }
}

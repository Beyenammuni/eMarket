using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Products.Events;
using eMarket.Domain.Catalog.Products.Rules;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Catalog.Products;

public sealed class Product : AggregateRoot<ProductId>
{
    private Product()
    {
    }

    private Product(
        ProductId id,
        ProductName name,
        ProductDescription description,
        Money price,
        Sku sku,
        CategoryId categoryId)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Sku = sku;
        CategoryId = categoryId;

        Status = ProductStatus.Draft;
        StockQuantity = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public ProductName Name { get; private set; } = default!;

    public ProductDescription Description { get; private set; } = default!;

    public Money Price { get; private set; } = default!;

    public Sku Sku { get; private set; } = default!;

    public CategoryId CategoryId { get; private set; } = default!;

    public ProductStatus Status { get; private set; }

    public int StockQuantity { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<Product> Create(
        ProductName name,
        ProductDescription description,
        Money price,
        Sku sku,
        CategoryId categoryId)
    {
        var product = new Product(
            ProductId.New(),
            name,
            description,
            price,
            sku,
            categoryId);

        product.AddDomainEvent(
            new ProductCreatedDomainEvent(product.Id));

        return Result<Product>.Success(product);
    }

    public Result Rename(ProductName newName)
    {
        if (Name == newName)
            return Result.Failure(ProductErrors.SameName);

        Name = newName;

        AddDomainEvent(
            new ProductRenamedDomainEvent(Id, newName));

        return Result.Success();
    }

    public Result ChangePrice(Money newPrice)
    {
        if (new ProductCannotHaveSamePriceRule(Price, newPrice).IsBroken())
        {
            return Result.Failure(ProductErrors.SamePrice);
        }

        if (new ProductPriceMustBePositiveRule(newPrice).IsBroken())
        {
            return Result.Failure(ProductErrors.ProductPriceMustBePositive);
        }
        Price = newPrice;

        AddDomainEvent(
            new ProductPriceChangedDomainEvent(Id, newPrice));

        return Result.Success();
    }

    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(ProductErrors.InvalidStock);

        StockQuantity += quantity;

        AddDomainEvent(
            new ProductStockIncreasedDomainEvent(Id, quantity));

        return Result.Success();
    }

    public Result RemoveStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(ProductErrors.InvalidStock);

        if (StockQuantity < quantity)
            return Result.Failure(ProductErrors.InvalidStock);

        StockQuantity -= quantity;

        AddDomainEvent(
            new ProductStockDecreasedDomainEvent(Id, quantity));

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == ProductStatus.Active)
            return Result.Failure(ProductErrors.AlreadyActive);

        Status = ProductStatus.Active;

        AddDomainEvent(
            new ProductActivatedDomainEvent(Id));

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == ProductStatus.Inactive)
            return Result.Failure(ProductErrors.AlreadyInactive);

        Status = ProductStatus.Inactive;

        AddDomainEvent(
            new ProductDeactivatedDomainEvent(Id));

        return Result.Success();
    }
}

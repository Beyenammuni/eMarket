using eMarket.Domain.Businesses;
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
        BusinessId businessId,
        CategoryId categoryId,
        ProductName name,
        ProductDescription description,
        Money price,
        Sku sku,
        string? imageUrl= null)
    {
        Id = id;
        BusinessId = businessId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        Sku = sku;
        ImageUrl = imageUrl;

        Status = ProductStatus.Draft;
        StockQuantity = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public BusinessId BusinessId { get; private set; } = default!;

    public CategoryId CategoryId { get; private set; } = default!;

    public ProductName Name { get; private set; } = default!;

    public ProductDescription Description { get; private set; } = default!;

    public Money Price { get; private set; } = default!;

    public Sku Sku { get; private set; } = default!;

    public string? ImageUrl { get; private set; }

    public ProductStatus Status { get; private set; }

    public int StockQuantity { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Result<Product> Create(
        BusinessId businessId,
        CategoryId categoryId,
        ProductName name,
        ProductDescription description,
        Money price,
        Sku sku,
        string? imageUrl = null)
    {
        var product = new Product(
            ProductId.New(),
            businessId,
            categoryId,
            name,
            description,
            price,
            sku,
            imageUrl);

        product.AddDomainEvent(
            new ProductCreatedDomainEvent(product.Id));

        return Result<Product>.Success(product);
    }

    public Result Rename(ProductName newName)
    {
        if (Name == newName)
        {
            return Result.Failure(ProductErrors.SameName);
        }

        Name = newName;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new ProductRenamedDomainEvent(Id, newName));

        return Result.Success();
    }
    public Result Delete()
    {
        if (IsDeleted)
            return Result.Failure(
                ProductErrors.AlreadyDeleted);

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

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
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new ProductPriceChangedDomainEvent(Id, newPrice));

        return Result.Success();
    }

    public Result IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(ProductErrors.InvalidStock);
        }

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new ProductStockIncreasedDomainEvent(Id, quantity));

        return Result.Success();
    }

    public Result DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(ProductErrors.InvalidStock);
        }

        if (StockQuantity < quantity)
        {
            return Result.Failure(ProductErrors.InvalidStock);
        }

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new ProductStockDecreasedDomainEvent(Id, quantity));

        return Result.Success();
    }

    public Result ChangeImage(string? imageUrl)
    {
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == ProductStatus.Active)
        {
            return Result.Failure(ProductErrors.AlreadyActive);
        }

        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new ProductActivatedDomainEvent(Id));

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == ProductStatus.Inactive)
        {
            return Result.Failure(ProductErrors.AlreadyInactive);
        }

        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new ProductDeactivatedDomainEvent(Id));

        return Result.Success();
    }
    public Result ChangeDescription(
    ProductDescription newDescription)
    {
        Description = newDescription;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
    public Result ChangeCategory(
       CategoryId newCategoryId)
    {
        if (CategoryId == newCategoryId)
        {
            return Result.Success();
        }

        CategoryId = newCategoryId;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
    public Result ChangeSku(Sku newSku)
    {
        if (Sku == newSku)
        {
            return Result.Success();
        }

        Sku = newSku;

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}

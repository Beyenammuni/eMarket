
using eMarket.Domain.Catalog.Products.ValueObjects;

namespace eMarket.Domain.Catalog.Products.Variants;

public sealed class ProductVariant
{
    private ProductVariant() { }

    private ProductVariant(
        Guid id,
        ProductId productId,
        string name,
        string? sku,
        decimal price,
        int stockQuantity)
    {
        Id = id;
        ProductId = productId;
        Name = name;
        Sku = sku;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public ProductId ProductId { get; private set; } = default!;

    public string Name { get; private set; } = string.Empty;

    public string? Sku { get; private set; }

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static ProductVariant Create(
        ProductId productId,
        string name,
        string? sku,
        decimal price,
        int stockQuantity)
    {
        ArgumentNullException.ThrowIfNull(productId);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Variant name is required.", nameof(name));

        if (name.Trim().Length > 100)
            throw new ArgumentException(
                "Variant name cannot exceed 100 characters.", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            sku = null;
        else
            sku = sku.Trim();

        if (sku?.Length > 50)
            throw new ArgumentException(
                "SKU cannot exceed 50 characters.", nameof(sku));

        if (price < 0)
            throw new ArgumentOutOfRangeException(
                nameof(price), "Price cannot be negative.");

        if (stockQuantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(stockQuantity), "Stock cannot be negative.");

        return new ProductVariant(
            Guid.NewGuid(),
            productId,
            name.Trim(),
            sku,
            price,
            stockQuantity);
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice));

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        StockQuantity = checked(StockQuantity + quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (quantity > StockQuantity)
            throw new InvalidOperationException(
                "Insufficient variant stock.");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeSku(string? newSku)
    {
        newSku = string.IsNullOrWhiteSpace(newSku)
            ? null
            : newSku.Trim();

        if (newSku?.Length > 50)
            throw new ArgumentException(
                "SKU cannot exceed 50 characters.", nameof(newSku));

        Sku = newSku;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

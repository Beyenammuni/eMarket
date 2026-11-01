
namespace eMarket.Domain.Catalog.Products.Options;

public sealed class ProductOption
{
    private ProductOption() { }

    public Guid Id { get; private set; }

    public Guid OptionGroupId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal AdditionalPrice { get; private set; }

    public bool IsActive { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static ProductOption Create(
        Guid optionGroupId,
        string name,
        decimal additionalPrice = 0,
        int displayOrder = 0)
    {
        if (optionGroupId == Guid.Empty)
            throw new ArgumentException(
                "Option group ID is required.", nameof(optionGroupId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Option name is required.", nameof(name));

        if (name.Trim().Length > 100)
            throw new ArgumentException(
                "Option name cannot exceed 100 characters.", nameof(name));

        if (additionalPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(additionalPrice),
                "Additional price cannot be negative.");

        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        return new ProductOption
        {
            Id = Guid.NewGuid(),
            OptionGroupId = optionGroupId,
            Name = name.Trim(),
            AdditionalPrice = additionalPrice,
            IsActive = true,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }
}

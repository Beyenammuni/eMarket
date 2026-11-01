
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Exceptions;

namespace eMarket.Domain.Catalog.Products.Options;

public sealed class ProductOptionGroup
{
    private ProductOptionGroup() { }

    private readonly List<ProductOption> _options = [];

    public Guid Id { get; private set; }

    public ProductId ProductId { get; private set; } = default!;

    public string Name { get; private set; } = string.Empty;

    public bool IsRequired { get; private set; }

    public int MinSelections { get; private set; }

    public int MaxSelections { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ProductOption> Options
        => _options.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }


    public void AddOption(ProductOption option)
    {
        ArgumentNullException.ThrowIfNull(option);

        if (option.OptionGroupId != Id)
        {
            throw new ArgumentException(
                "The option belongs to a different option group.",
                nameof(option));
        }

        if (_options.Any(x =>
            string.Equals(
                x.Name,
                option.Name,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException(
      "An option with this name already exists in the group.");
        }

        _options.Add(option);
        UpdatedAt = DateTime.UtcNow;
    }

    public static ProductOptionGroup Create(
        ProductId productId,
        string name,
        bool isRequired,
        int minSelections,
        int maxSelections,
        int displayOrder = 0)
    {
        ArgumentNullException.ThrowIfNull(productId);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Option group name is required.", nameof(name));

        if (name.Trim().Length > 100)
            throw new ArgumentException(
                "Option group name cannot exceed 100 characters.",
                nameof(name));

        if (minSelections < 0)
            throw new ArgumentOutOfRangeException(nameof(minSelections));

        if (maxSelections < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSelections));

        if (minSelections > maxSelections)
            throw new ArgumentException(
                "Minimum selections cannot exceed maximum selections.");

        if (isRequired && minSelections < 1)
            throw new ArgumentException(
                "Required groups must have at least one minimum selection.");

        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        return new ProductOptionGroup
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Name = name.Trim(),
            IsRequired = isRequired,
            MinSelections = minSelections,
            MaxSelections = maxSelections,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

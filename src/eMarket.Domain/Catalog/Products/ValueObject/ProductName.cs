using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog.Products.ValueObjects;

public sealed class ProductName : ValueObject
{
    public string Value { get; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name is required.");

        value = value.Trim();

        if (value.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters.");

        return new ProductName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

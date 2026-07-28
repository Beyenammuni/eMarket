using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog.Products.ValueObjects;

public sealed class ProductDescription : ValueObject
{
    public string Value { get; }

    private ProductDescription(string value)
    {
        Value = value;
    }

    public static ProductDescription Create(string value)
    {
        value ??= string.Empty;

        value = value.Trim();

        if (value.Length > 4000)
            throw new ArgumentException("Product description cannot exceed 4000 characters.");

        return new ProductDescription(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

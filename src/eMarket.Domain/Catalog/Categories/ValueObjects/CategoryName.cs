using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog.Categories.ValueObjects;

public sealed class CategoryName : ValueObject
{
    public string Value { get; }

    private CategoryName(string value)
    {
        Value = value;
    }

    public static CategoryName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Category name is required.");

        value = value.Trim();

        if (value.Length > 100)
            throw new ArgumentException("Category name is too long.");

        return new CategoryName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}

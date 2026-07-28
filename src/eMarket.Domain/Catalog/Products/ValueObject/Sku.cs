using System.Text.RegularExpressions;
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog.Products.ValueObjects;

public sealed class Sku : ValueObject
{
    private static readonly Regex Regex =
        new(@"^[A-Z0-9\-_]{3,50}$", RegexOptions.Compiled);

    public string Value { get; }

    private Sku(string value)
    {
        Value = value;
    }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU is required.");

        value = value.Trim().ToUpperInvariant();

        if (!Regex.IsMatch(value))
            throw new ArgumentException("Invalid SKU format.");

        return new Sku(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}

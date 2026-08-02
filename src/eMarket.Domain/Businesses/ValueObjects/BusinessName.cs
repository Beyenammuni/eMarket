using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses.ValueObjects;

public sealed class BusinessName : ValueObject
{
    private BusinessName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BusinessName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Business name is required.");

        value = value.Trim();

        if (value.Length > 150)
            throw new ArgumentException("Business name cannot exceed 150 characters.");

        return new BusinessName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}

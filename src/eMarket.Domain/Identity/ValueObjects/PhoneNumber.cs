using System.Text.RegularExpressions;
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Identity.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex Regex =
        new(@"^\+?[1-9]\d{7,14}$");

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number is required.");

        value = value.Trim();

        if (!Regex.IsMatch(value))
            throw new ArgumentException("Invalid phone number.");

        return new PhoneNumber(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value;
}

using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Identity.ValueObjects;

public sealed class FullName : ValueObject
{
    public string FirstName { get; }

    public string LastName { get; }

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static FullName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.");

        return new FullName(
            firstName.Trim(),
            lastName.Trim());
    }

    public string Full => $"{FirstName} {LastName}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => Full;
}

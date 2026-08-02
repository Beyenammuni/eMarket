using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Sales.Carts.ValueObjects;

public sealed class Quantity : ValueObject
{
    private Quantity(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Quantity Create(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        return new Quantity(value);
    }

    public Quantity Increase(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Increase amount must be greater than zero.");

        return new Quantity(Value + amount);
    }

    public Quantity Decrease(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Decrease amount must be greater than zero.");

        if (Value - amount <= 0)
            throw new ArgumentException("Quantity cannot be zero or negative.");

        return new Quantity(Value - amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
        => Value.ToString();
}

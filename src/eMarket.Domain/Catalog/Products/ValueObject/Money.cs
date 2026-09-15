using eMarket.Domain.Common;
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Catalog.Products.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.");


        return new Money(amount, currency);
    }

    public static Money Zero(Currency currency)
        => Create(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);

        return new Money(
            Amount + other.Amount,
            Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        if (Amount < other.Amount)
            throw new InvalidOperationException(
                "Money cannot be negative.");

        return new Money(
            Amount - other.Amount,
            Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException(
                "Factor cannot be negative.");

        return new Money(
            Amount * factor,
            Currency);
    }

    public bool IsZero()
        => Amount == 0;

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);

        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        EnsureSameCurrency(other);

        return Amount < other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                "Currencies must match.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
        => $"{Amount:0.00} {Currency}";
}

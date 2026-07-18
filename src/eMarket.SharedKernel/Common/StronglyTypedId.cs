namespace eMarket.SharedKernel.Common;

public abstract record StronglyTypedId(Guid Value)
{
    public override string ToString()
        => Value.ToString();

    public static Guid operator +(StronglyTypedId id)
        => id.Value;
}
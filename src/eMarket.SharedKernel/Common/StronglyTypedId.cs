namespace eMarket.SharedKernel.Common;

public abstract record StronglyTypedId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString()
        => Value.ToString();

    public static implicit operator Guid(StronglyTypedId id)
        => id.Value;
}

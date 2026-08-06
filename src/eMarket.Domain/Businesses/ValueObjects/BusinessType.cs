using EMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses.ValueObjects;

public sealed class BusinessType : Enumeration
{
    public static readonly BusinessType Grocery =
        new(1, nameof(Grocery));

    public static readonly BusinessType Bakery =
        new(2, nameof(Bakery));

    public static readonly BusinessType Restaurant =
        new(3, nameof(Restaurant));

    public static readonly BusinessType Pharmacy =
        new(4, nameof(Pharmacy));

    public static readonly BusinessType Warehouse =
        new(5, nameof(Warehouse));

    private BusinessType(int id, string name)
        : base(id, name)
    {
    }
}

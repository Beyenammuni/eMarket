using eMarket.SharedKernel.Common;
using EMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses.ValueObjects;

public sealed class BusinessRole : Enumeration
{
    public static readonly BusinessRole Owner =
        new(1, nameof(Owner));

    public static readonly BusinessRole Manager =
        new(2, nameof(Manager));

    public static readonly BusinessRole Cashier =
        new(3, nameof(Cashier));

    public static readonly BusinessRole InventoryManager =
        new(4, nameof(InventoryManager));

    public static readonly BusinessRole DeliveryManager =
        new(5, nameof(DeliveryManager));

    public static readonly BusinessRole Accountant =
        new(6, nameof(Accountant));

    public static readonly BusinessRole Employee =
        new(7, nameof(Employee));

    private BusinessRole(int id, string name)
        : base(id, name)
    {
    }
}

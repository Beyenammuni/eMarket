using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Identity;

public sealed class UserRole : Enumeration
{
    public static readonly UserRole Customer = new(1, nameof(Customer));
    public static readonly UserRole Seller = new(2, nameof(Seller));
    public static readonly UserRole DeliveryDriver = new(3, nameof(DeliveryDriver));
    public static readonly UserRole Admin = new(4, nameof(Admin));
    public static readonly UserRole StoreManager = new(5, nameof(StoreManager));
    public static readonly UserRole SuperAdmin = new(6, nameof(SuperAdmin));

    private UserRole(int id, string name) : base(id, name)
    {
    }
}

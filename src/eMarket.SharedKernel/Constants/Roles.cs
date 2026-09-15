namespace eMarket.SharedKernel.Constants;

public static class Roles
{
    public const string SuperAdmin = nameof(SuperAdmin);

    public const string Admin = nameof(Admin);

    public const string Seller = nameof(Seller);

    public const string Customer = nameof(Customer);

    public const string DeliveryDriver = nameof(DeliveryDriver);

    public const string StoreManager = nameof(StoreManager);

    public static readonly IReadOnlyList<string> All =
    [
        SuperAdmin,
        Admin,
        Seller,
        Customer,
        DeliveryDriver,
        StoreManager
    ];
}
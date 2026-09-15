using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Businesses.ValueObjects;

public sealed class MerchantType : Enumeration
{
    public static readonly MerchantType Personal =
        new(1, nameof(Personal));

    public static readonly MerchantType PrivateCompany =
        new(2, nameof(PrivateCompany));

    public static readonly MerchantType LimitedOrJointStockCompany =
        new(3, nameof(LimitedOrJointStockCompany));

    private MerchantType(
        int id,
        string name)
        : base(id, name)
    {
    }
}


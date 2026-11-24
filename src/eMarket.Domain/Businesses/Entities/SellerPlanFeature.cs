namespace eMarket.Domain.Businesses.Entities;

public sealed class SellerPlanFeature
{
    private SellerPlanFeature()
    {
    }

    public Guid SellerPlanId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public static SellerPlanFeature Create(
        Guid sellerPlanId,
        string code)
    {
        if (sellerPlanId == Guid.Empty)
            throw new ArgumentException("SellerPlanId is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Feature code is required.");

        return new SellerPlanFeature
        {
            SellerPlanId = sellerPlanId,
            Code = code.Trim()
        };
    }
}

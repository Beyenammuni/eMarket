using eMarket.SharedKernel.Guards;

namespace eMarket.SharedKernel.Guards;

public static class BusinessGuardExtension
{
    public static decimal ValidDiscount(
        this IGuardClause _,
        decimal discount,
        string parameterName)
    {
        if (discount < 0 || discount > 100)
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Discount must be between 0 and 100.");

        return discount;
    }

    public static int PositiveStock(
        this IGuardClause _,
        int stock,
        string parameterName)
    {
        if (stock < 0)
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Stock cannot be negative.");

        return stock;
    }
}
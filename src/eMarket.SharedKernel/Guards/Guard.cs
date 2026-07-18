using eMarket.SharedKernel.Guards;

namespace eMarket.SharedKernel.Guards;
public sealed class Guard : IGuardClause
{
    private Guard()
    {
    }

    public static IGuardClause Against { get; } = new Guard();
}
using eMarket.SharedKernel.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace eMarket.Infrastructure.Persistence.Converters;

public sealed class StronglyTypedIdComparer<TStronglyTypedId>
    : ValueComparer<TStronglyTypedId>
    where TStronglyTypedId : StronglyTypedId
{
    public StronglyTypedIdComparer()
        : base(
            (x, y) => x!.Value == y!.Value,
            id => id.Value.GetHashCode(),
            id => id)
    {
    }
}

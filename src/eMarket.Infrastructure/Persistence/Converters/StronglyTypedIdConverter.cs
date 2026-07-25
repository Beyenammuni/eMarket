using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using eMarket.SharedKernel.Common;

namespace eMarket.Infrastructure.Persistence.Converters;

public sealed class StronglyTypedIdConverter<TStronglyTypedId>
    : ValueConverter<TStronglyTypedId, Guid>
    where TStronglyTypedId : StronglyTypedId
{
    public StronglyTypedIdConverter(
        Func<Guid, TStronglyTypedId> factory)
        : base(
            id => id.Value,
            value => factory(value))
    {
    }
}

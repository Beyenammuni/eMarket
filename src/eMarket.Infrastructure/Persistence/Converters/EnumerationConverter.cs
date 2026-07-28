using EMarket.SharedKernel.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace eMarket.Infrastructure.Persistence.Converters;

public sealed class EnumerationConverter<TEnumeration>
    : ValueConverter<TEnumeration, int>
    where TEnumeration : Enumeration
{
    public EnumerationConverter()
        : base(
            enumeration => enumeration.Id,
            id => Enumeration.FromId<TEnumeration>(id))
    {
    }
}

using eMarket.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Sales.Carts
{
    public sealed record CartId(Guid Value): StronglyTypedId(Value)
    {
        public static CartId New() => new(Guid.NewGuid());
        public static CartId Create(Guid value) => new(value);
    }
}

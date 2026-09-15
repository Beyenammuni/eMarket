using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Domain.Sales.Carts
{
    public enum CartStatus
    {
        Active = 1,
        CheckedOut = 2,
        Abandoned = 3
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Sales.Carts.Commands.UpdateCartItem
{
    public sealed record UpdateCartItemRequest(int Quantity);
}

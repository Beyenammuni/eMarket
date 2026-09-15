using eMarket.Domain.Businesses;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Products.Commands.AddStock
{
    public sealed record StockRequest(Guid BusinessId, int Quantity);
}

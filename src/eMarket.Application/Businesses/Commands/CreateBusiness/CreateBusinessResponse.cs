using eMarket.Domain.Businesses.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Businesses.Commands.CreateBusiness
{
   public sealed record CreateBusinessResponse(Guid Id, string Name, int Type, string TypeName);
}

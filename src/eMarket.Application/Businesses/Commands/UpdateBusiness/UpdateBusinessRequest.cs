using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Businesses.Commands.UpdateBusiness
{
    public sealed record UpdateBusinessRequest(
        string Name,
        int Type);
}

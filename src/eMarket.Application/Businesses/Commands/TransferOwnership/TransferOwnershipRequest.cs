using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Businesses.Commands.TransferOwnership
{
    public sealed record TransferOwnershipRequest(
    Guid NewOwnerId);
}

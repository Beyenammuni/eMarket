using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Businesses.Commands.ChangeMemberRole
{
    public sealed record ChangeMemberRoleRequest(
        int Role);
}

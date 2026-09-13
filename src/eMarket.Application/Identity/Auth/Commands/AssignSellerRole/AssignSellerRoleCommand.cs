using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Identity.Auth.Commands.AssignSellerRole
{
    public sealed record AssignSellerRoleCommand(
        Guid UserId)
        : IRequest<Result>;
}

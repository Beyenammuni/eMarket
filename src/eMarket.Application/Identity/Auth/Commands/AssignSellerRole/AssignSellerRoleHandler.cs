using eMarket.Application.Common.Interfaces;
using eMarket.Application.Identity.Auth.Commands.AssignSellerRole;
using eMarket.Domain.Identity;
using eMarket.Domain.Identity.Entities;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Identity.Commands.AssignSellerRole;

internal sealed class AssignSellerRoleCommandHandler(
    IIdentityDbContext context)
    : IRequestHandler<AssignSellerRoleCommand, Result>
{
    public async Task<Result> Handle(
        AssignSellerRoleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = UserId.Create(request.UserId);

        var userExists = await context.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            return Result.Failure(
                new Error(
                    "User.NotFound",
                    "User was not found."));
        }

        var sellerRoleExists = await context.UserRoleAssignments
            .AnyAsync(
                x =>
                    x.UserId == userId &&
                    x.RoleId == UserRole.Seller.Id,
                cancellationToken);

        if (sellerRoleExists)
        {
            return Result.Failure(
                new Error(
                    "UserRole.AlreadySeller",
                    "User already has the Seller role."));
        }

        await context.UserRoleAssignments.AddAsync(
            new UserRoleAssignment(
                userId,
                UserRole.Seller),
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

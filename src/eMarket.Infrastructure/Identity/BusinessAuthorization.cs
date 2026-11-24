
using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Infrastructure.Identity;

public sealed class BusinessAuthorization : IBusinessAuthorization
{
    private readonly AppDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;

    public BusinessAuthorization(
        AppDbContext context,
        ICurrentUser currentUser,
        ISellerPlanFeatureService sellerPlanFeatureService)
    {
        _context = context;
        _currentUser = currentUser;
        _sellerPlanFeatureService = sellerPlanFeatureService;
    }

    public Task<bool> IsMemberAsync(
        BusinessId businessId,
        UserId userId,
        CancellationToken cancellationToken = default)
        => _context.BusinessMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BusinessId == businessId &&
                     x.UserId == userId &&
                     x.IsActive,
                cancellationToken);

    public Task<bool> HasRoleAsync(
        BusinessId businessId,
        UserId userId,
        BusinessRole role,
        CancellationToken cancellationToken = default)
        => _context.BusinessMembers
            .AsNoTracking()
            .AnyAsync(
                x => x.BusinessId == businessId &&
                     x.UserId == userId &&
                     x.Role == role &&
                     x.IsActive,
                cancellationToken);

    public async Task<bool> HasPermissionAsync(
        BusinessId businessId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsInRole(eMarket.SharedKernel.Constants.Roles.SuperAdmin) ||
            _currentUser.IsInRole(eMarket.SharedKernel.Constants.Roles.Admin))
        {
            return true;
        }

        var userId = _currentUser.UserId;

        if (userId is null)
            return false;

        var member = await _context.BusinessMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.BusinessId == businessId &&
                     x.UserId == userId &&
                     x.IsActive,
                cancellationToken);

        if (member is null)
            return false;

        // Owner permissions are never customized.
        if (member.Role.Name == BusinessRole.Owner.Name)
        {
            return BusinessRolePermissions.HasPermission(
                member.Role,
                permission);
        }

        var hasAdvancedManagement =
            await _sellerPlanFeatureService.HasFeatureAsync(
                businessId.Value,
                SellerPlanFeatureCodes.Staff.AdvancedManagement,
                cancellationToken);

        if (hasAdvancedManagement)
        {
            var customPermission = await _context.BusinessMemberPermissions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.BusinessMemberId == member.Id &&
                         x.Permission == permission,
                    cancellationToken);

            if (customPermission is not null)
                return customPermission.IsGranted;
        }

        return BusinessRolePermissions.HasPermission(
            member.Role,
            permission);
    }

    public async Task<bool> HasPermissionOnAnyBusinessAsync(
        string permission,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
            return false;

        var businessRoles = await _context.BusinessMembers
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive)
            .Select(x => x.Role)
            .ToListAsync(cancellationToken);

        return businessRoles.Any(role =>
            BusinessRolePermissions.HasPermission(
                role,
                permission));
    }
}


using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Commands.SetMemberPermission;

internal sealed class SetMemberPermissionCommandHandler
    : IRequestHandler<SetMemberPermissionCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessDbContext _businessDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;

    public SetMemberPermissionCommandHandler(
        IBusinessRepository businessRepository,
        IBusinessDbContext businessDbContext,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ISellerPlanFeatureService sellerPlanFeatureService)
    {
        _businessRepository = businessRepository;
        _businessDbContext = businessDbContext;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _sellerPlanFeatureService = sellerPlanFeatureService;
    }

    public async Task<Result> Handle(
        SetMemberPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        // Only the business owner can customize staff permissions.
        var currentUserId = _currentUser.UserId;

        if (currentUserId is null)
            return Result.Failure(BusinessErrors.UserNotAuthenticated);

        var currentMember = await _businessRepository.GetMemberAsync(
            businessId,
            currentUserId,
            cancellationToken);

        if (currentMember is null ||
            currentMember.Role != BusinessRole.Owner)
        {
            return Result.Failure(BusinessErrors.NotOwner);
        }

        // Custom permissions require the PRO feature.
        var hasFeature = await _sellerPlanFeatureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Staff.AdvancedManagement,
            cancellationToken);

        if (!hasFeature)
            return Result.Failure(BusinessErrors.Forbidden);

        // Only explicitly allowed permissions can be customized.
        if (!StaffPermissionPolicy.CanCustomize(request.Permission))
            return Result.Failure(BusinessErrors.Forbidden);

        var targetMember = await _businessRepository.GetMemberAsync(
            businessId,
            UserId.Create(request.UserId),
            cancellationToken);

        if (targetMember is null)
            return Result.Failure(BusinessErrors.MemberNotFound);

        // Owner permissions cannot be customized.
        if (targetMember.Role == BusinessRole.Owner)
            return Result.Failure(BusinessErrors.Forbidden);

        var customPermission =
            await _businessDbContext.BusinessMemberPermissions
                .FirstOrDefaultAsync(
                    x => x.BusinessMemberId == targetMember.Id &&
                         x.Permission == request.Permission,
                    cancellationToken);

        if (customPermission is null)
        {
            customPermission = BusinessMemberPermission.Create(
                targetMember.Id,
                request.Permission,
                request.IsGranted);

            _businessDbContext.BusinessMemberPermissions.Add(
                customPermission);
        }
        else
        {
            customPermission.SetGranted(request.IsGranted);
        }

        _businessDbContext.BusinessActivityLogs.Add(
     BusinessActivityLog.Create(
         businessId,
         currentUserId,
         targetMember.UserId,
         "MemberPermissionChanged",
         $"تم {(request.IsGranted ? "Granted" : "Canceled")} Permission: {request.Permission}"
     ));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

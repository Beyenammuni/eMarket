using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using static eMarket.SharedKernel.Constants.Permissions;

namespace eMarket.Application.Businesses.Commands.ChangeMemberRole;

internal sealed class ChangeMemberRoleCommandHandler
    : IRequestHandler<
        ChangeMemberRoleCommand,
        Result<ChangeMemberRoleResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessAuthorization _authorization;
    private readonly IBusinessDbContext _businessDbContext;
    private readonly ICurrentUser _currentUser;

    public ChangeMemberRoleCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        IBusinessAuthorization authorization,
        IBusinessDbContext businessDbContext,
        ICurrentUser currentUser)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _businessDbContext = businessDbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<ChangeMemberRoleResponse>> Handle(
        ChangeMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var businessId =
            BusinessId.Create(request.BusinessId);

        var hasPermission =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Business.ManageRoles,
                cancellationToken);

        if (!hasPermission)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.Forbidden);
        }

        var business =
            await _businessRepository.GetWithMembersAsync(
                businessId,
                cancellationToken);

        if (business is null)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.NotFound);
        }

        BusinessRole role;

        try
        {
            role =
                BusinessRole.FromValue<BusinessRole>(request.Role);
        }
        catch
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.UserNotMember);
        }

        if (role == BusinessRole.Owner)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.TransferToSameOwner);
        }
        var actorUserId = _currentUser.UserId;

        if (actorUserId is null)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.UserNotAuthenticated);
        }

        var targetUserId = UserId.Create(request.UserId);

        var targetMember = await _businessRepository.GetMemberAsync(
            businessId,
            targetUserId,
            cancellationToken);

        if (targetMember is null)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.UserNotMember);
        }

        var oldRole = targetMember.Role.Name;

        var result =
    business.ChangeMemberRole(
        targetUserId,
        role);

        if (result.IsFailure)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                result.Error);
        }

        _businessDbContext.BusinessActivityLogs.Add(
    BusinessActivityLog.Create(
        businessId,
        actorUserId,
        targetUserId,
        "MemberRoleChanged",
        $"Employee role changed from {oldRole} to {role.Name}"
    ));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChangeMemberRoleResponse>.Success(
            new ChangeMemberRoleResponse(
                request.BusinessId,
                request.UserId,
                role.Id));
    }
}

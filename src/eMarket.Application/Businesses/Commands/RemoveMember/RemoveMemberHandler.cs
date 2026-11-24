using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.RemoveMember;

internal sealed class RemoveMemberHandler
    : IRequestHandler<RemoveMemberCommand, Result<RemoveMemberResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessAuthorization _authorization;
    private readonly IBusinessDbContext _businessDbContext;
    private readonly ICurrentUser _currentUser;
    public RemoveMemberHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork,
        IBusinessAuthorization authorization,
        IBusinessDbContext businessDbContext,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _businessDbContext = businessDbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<RemoveMemberResponse>> Handle(
        RemoveMemberCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        var hasPermission = await _authorization.HasPermissionAsync(
            businessId,
            Permissions.Business.ManageMembers,
            cancellationToken);

        if (!hasPermission)
        {
            return Result<RemoveMemberResponse>.Failure(
                BusinessErrors.Forbidden);
        }

        var business = await _repository.GetWithMembersAsync(
            businessId,
            cancellationToken);

        if (business is null)
        {
            return Result<RemoveMemberResponse>.Failure(
                BusinessErrors.NotFound);
        }
        var actorUserId = _currentUser.UserId;

        if (actorUserId is null)
        {
            return Result<RemoveMemberResponse>.Failure(
                BusinessErrors.UserNotAuthenticated);
        }

        var targetUserId = UserId.Create(request.UserId);

        var result = business.RemoveMember(targetUserId);

        if (result.IsFailure)
        {
            return Result<RemoveMemberResponse>.Failure(result.Error);
        }

        _businessDbContext.BusinessActivityLogs.Add(
    BusinessActivityLog.Create(
        businessId,
        actorUserId,
        targetUserId,
        "MemberRemoved",
        "A business member was removed."
    ));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RemoveMemberResponse>.Success(
            new RemoveMemberResponse(
                request.BusinessId,
                request.UserId));
    }
}

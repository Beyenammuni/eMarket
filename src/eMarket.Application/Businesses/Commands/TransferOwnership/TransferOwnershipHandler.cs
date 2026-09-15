using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using static eMarket.SharedKernel.Constants.Permissions;

namespace eMarket.Application.Businesses.Commands.TransferOwnership;

internal sealed class TransferOwnershipCommandHandler
    : IRequestHandler<
        TransferOwnershipCommand,
        Result<TransferOwnershipResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessAuthorization _authorization;
    private readonly ICurrentUser _currentUser;

    public TransferOwnershipCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        IBusinessAuthorization authorization,
        ICurrentUser currentUser)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _currentUser = currentUser;
    }

    public async Task<Result<TransferOwnershipResponse>> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        if (currentUserId is null)
        {
            return Result<TransferOwnershipResponse>.Failure(
                BusinessErrors.Unauthorized);
        }

        var businessId =
            BusinessId.Create(request.BusinessId);

        var hasPermission =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Business.TransferOwnership,
                cancellationToken);

        if (!hasPermission)
        {
            return Result<TransferOwnershipResponse>.Failure(
                BusinessErrors.Forbidden);
        }

        var business =
            await _businessRepository.GetWithMembersAsync(
                businessId,
                cancellationToken);

        if (business is null)
        {
            return Result<TransferOwnershipResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var result =
            business.TransferOwnership(
                currentUserId,
                UserId.Create(request.NewOwnerId));

        if (result.IsFailure)
        {
            return Result<TransferOwnershipResponse>.Failure(
                result.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<TransferOwnershipResponse>.Success(
            new TransferOwnershipResponse(
                request.BusinessId,
                request.NewOwnerId));
    }
}

using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static eMarket.SharedKernel.Constants.Permissions;

namespace eMarket.Application.Businesses.Commands.AddBusinessMember;

internal sealed class AddBusinessMemberCommandHandler
    : IRequestHandler<
        AddBusinessMemberCommand,
        Result<AddBusinessMemberResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IIdentityDbContext _identityContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessAuthorization _authorization;

    public AddBusinessMemberCommandHandler(
        IBusinessRepository businessRepository,
        IIdentityDbContext identityContext,
        IUnitOfWork unitOfWork,
        IBusinessAuthorization authorization)
    {
        _businessRepository = businessRepository;
        _identityContext = identityContext;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
    }

    public async Task<Result<AddBusinessMemberResponse>> Handle(
        AddBusinessMemberCommand request,
        CancellationToken cancellationToken)
    {
        var businessId =
            BusinessId.Create(request.BusinessId);

        var hasPermission =
            await _authorization.HasPermissionAsync(
                businessId,
                Permissions.Business.ManageMembers,
                cancellationToken);

        if (!hasPermission)
        {
            return Result<AddBusinessMemberResponse>.Failure(
                BusinessErrors.Forbidden);
        }

        var business =
            await _businessRepository.GetWithMembersAsync(
                businessId,
                cancellationToken);

        if (business is null)
        {
            return Result<AddBusinessMemberResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var userId =
            UserId.Create(request.UserId);

        var userExists =
            await _identityContext.Users
                .AnyAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (!userExists)
        {
            return Result<AddBusinessMemberResponse>.Failure(
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
            return Result<AddBusinessMemberResponse>.Failure(
                BusinessErrors.UserNotMember);
        }



        var result =
            business.AddMember(
                userId,
                role);

        if (result.IsFailure)
        {
            return Result<AddBusinessMemberResponse>.Failure(
                result.Error);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<AddBusinessMemberResponse>.Success(
            new AddBusinessMemberResponse(
                request.BusinessId,
                request.UserId,
                role.Id));
    }
}

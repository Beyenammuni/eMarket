using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;
using EMarket.SharedKernel.Common;
using MediatR;

namespace eMarket.Application.Businesses.Commands.ChangeMemberRole;

internal sealed class ChangeMemberRoleHandler
    : IRequestHandler<ChangeMemberRoleCommand, Result<ChangeMemberRoleResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeMemberRoleHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ChangeMemberRoleResponse>> Handle(
        ChangeMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _repository.GetWithMembersAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
        {
            return Result<ChangeMemberRoleResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var role = Enumeration.FromValue<BusinessRole>(request.Role);

        var result = business.ChangeMemberRole(
            UserId.Create(request.UserId),
            role);

        if (result.IsFailure)
        {
            return Result<ChangeMemberRoleResponse>.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChangeMemberRoleResponse>.Success(
            new ChangeMemberRoleResponse(
                business.Id.Value,
                request.UserId,
                role.Name));
    }
}

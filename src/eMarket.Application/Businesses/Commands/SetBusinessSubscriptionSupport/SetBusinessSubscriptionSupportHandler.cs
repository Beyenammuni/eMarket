using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.SetBusinessSubscriptionSupport;

internal sealed class SetBusinessSubscriptionSupportHandler
    : IRequestHandler<SetBusinessSubscriptionSupportCommand, Result<bool>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessAuthorization _authorization;

    public SetBusinessSubscriptionSupportHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork,
        IBusinessAuthorization authorization)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
    }

    public async Task<Result<bool>> Handle(
        SetBusinessSubscriptionSupportCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        var hasPermission = await _authorization.HasPermissionAsync(
            businessId,
            Permissions.Business.Update,
            cancellationToken);

        if (!hasPermission)
        {
            return Result<bool>.Failure(BusinessErrors.Forbidden);
        }

        var business = await _repository.GetByIdAsync(
            businessId,
            cancellationToken);

        if (business is null)
        {
            return Result<bool>.Failure(BusinessErrors.NotFound);
        }

        var result = business.SetSubscriptionSupport(request.Enabled);

        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(request.Enabled);
    }
}

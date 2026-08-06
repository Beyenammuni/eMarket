using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.TransferOwnership;

internal sealed class TransferOwnershipHandler
    : IRequestHandler<TransferOwnershipCommand, Result<TransferOwnershipResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public TransferOwnershipHandler(
        IBusinessRepository repository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TransferOwnershipResponse>> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _repository.GetWithMembersAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
        {
            return Result<TransferOwnershipResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var result = business.TransferOwnership(
            _currentUser.UserId,
            UserId.Create(request.NewOwnerId));

        if (result.IsFailure)
        {
            return Result<TransferOwnershipResponse>.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TransferOwnershipResponse>.Success(
            new TransferOwnershipResponse(
                business.Id.Value,
                request.NewOwnerId));
    }
}

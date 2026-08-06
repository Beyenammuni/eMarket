using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.RemoveMember;

internal sealed class RemoveMemberHandler
    : IRequestHandler<RemoveMemberCommand, Result<RemoveMemberResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveMemberHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RemoveMemberResponse>> Handle(
        RemoveMemberCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _repository.GetWithMembersAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
        {
            return Result<RemoveMemberResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var result = business.RemoveMember(
            UserId.Create(request.UserId));

        if (result.IsFailure)
        {
            return Result<RemoveMemberResponse>.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RemoveMemberResponse>.Success(
            new RemoveMemberResponse(
                request.BusinessId,
                request.UserId));
    }
}

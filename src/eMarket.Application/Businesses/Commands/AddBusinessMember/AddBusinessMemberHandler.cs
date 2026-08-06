using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;
using EMarket.SharedKernel.Common;
using MediatR;

namespace eMarket.Application.Businesses.Commands.AddBusinessMember;

internal sealed class AddBusinessMemberHandler
    : IRequestHandler<AddBusinessMemberCommand, Result<AddBusinessMemberResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBusinessMemberHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AddBusinessMemberResponse>> Handle(
        AddBusinessMemberCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _repository.GetByIdAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
        {
            return Result<AddBusinessMemberResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var role = Enumeration.FromValue<BusinessRole>(request.Role);

        var result = business.AddMember(
            UserId.Create(request.UserId),
            role);

        if (result.IsFailure)
        {
            return Result<AddBusinessMemberResponse>.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new AddBusinessMemberResponse(
                business.Id.Value,
                request.UserId,
                role.Name));
    }
}

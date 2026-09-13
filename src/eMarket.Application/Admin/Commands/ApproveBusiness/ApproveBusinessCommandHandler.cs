using eMarket.Application.Admin.Commands.ApproveBusiness;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Admin.Businesses.Commands.ApproveBusiness;

public sealed class ApproveBusinessCommandHandler
    : IRequestHandler<ApproveBusinessCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ApproveBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByIdAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
        {
            return Result.Failure(
                BusinessErrors.NotFound);
        }

        if (business.Status != BusinessStatus.Pending)
        {
            return Result.Failure(
                new Error(
                    "Business.Status.InvalidApproval",
                    "Only pending businesses can be approved."));
        }

        var result = business.Activate();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

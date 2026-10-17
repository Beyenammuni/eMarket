
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.SuspendBusiness;

public sealed class SuspendBusinessCommandHandler
    : IRequestHandler<SuspendBusinessCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SuspendBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByIdAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
            return Result.Failure(BusinessErrors.NotFound);

        var result = business.Suspend();

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

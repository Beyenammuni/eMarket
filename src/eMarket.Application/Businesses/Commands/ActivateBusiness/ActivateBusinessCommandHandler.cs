
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.ActivateBusiness;

public sealed class ActivateBusinessCommandHandler
    : IRequestHandler<ActivateBusinessCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ActivateBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _businessRepository.GetByIdAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
            return Result.Failure(BusinessErrors.NotFound);

        if (business.Status == BusinessStatus.Closed)
            return Result.Failure(BusinessErrors.AlreadyClosed);

        var result = business.Activate();

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

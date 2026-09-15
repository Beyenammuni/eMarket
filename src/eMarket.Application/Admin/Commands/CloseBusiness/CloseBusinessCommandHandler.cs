using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Admin.Commands.CloseBusiness;

public sealed class CloseBusinessCommandHandler
    : IRequestHandler<CloseBusinessCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CloseBusinessCommand request,
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

        var result = business.Close();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

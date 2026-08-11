using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;
using EMarket.SharedKernel.Common;
using MediatR;

namespace eMarket.Application.Businesses.Commands.UpdateBusiness;

internal sealed class UpdateBusinessHandler
    : IRequestHandler<UpdateBusinessCommand, Result<UpdateBusinessResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBusinessHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateBusinessResponse>> Handle(
        UpdateBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var business = await _repository.GetByIdAsync(
            BusinessId.Create(request.BusinessId),
            cancellationToken);

        if (business is null)
        {
            return Result<UpdateBusinessResponse>.Failure(
                BusinessErrors.NotFound);
        }

        var result = business.Update(
            BusinessName.Create(request.Name),
            Enumeration.FromValue<BusinessType>(request.Type));

        if (result.IsFailure)
        {
            return Result<UpdateBusinessResponse>.Failure(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateBusinessResponse>.Success(
            new UpdateBusinessResponse(
                business.Id.Value,
                business.Name.Value,
                business.Type.Name));
    }
}

using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Results;
using EMarket.SharedKernel.Common;
using MediatR;

namespace eMarket.Application.Businesses.Commands.CreateBusiness;

public sealed class CreateBusinessCommandHandler
    : IRequestHandler<CreateBusinessCommand, Result<CreateBusinessResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateBusinessResponse>> Handle(
        CreateBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var businessName = BusinessName.Create(request.Name);

        if (await _businessRepository.ExistsAsync(businessName))
        {
            return Result<CreateBusinessResponse>.Failure(
                BusinessErrors.AlreadyExists);
        }


        var businessType = Enumeration.FromValue<BusinessType>(request.Type);

        var result = Business.Create(
            businessName,
            businessType,
            _currentUser.UserId);

        if (result.IsFailure)
        {
            return Result<CreateBusinessResponse>.Failure(result.Error);
        }

        await _businessRepository.AddAsync(result.Value, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateBusinessResponse>.Success(
            new CreateBusinessResponse(
                result.Value.Id.Value,
                result.Value.Name.Value,
                result.Value.Type.Id,
                result.Value.Type.Name
                ));
    }
}

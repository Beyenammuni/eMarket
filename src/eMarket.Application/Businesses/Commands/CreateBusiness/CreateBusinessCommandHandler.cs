using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.CreateBusiness;

public sealed class CreateBusinessCommandHandler
    : IRequestHandler<CreateBusinessCommand, Result<CreateBusinessResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ISubMerchantProvisioningQueue _subMerchantProvisioningQueue;

    public CreateBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ISubMerchantProvisioningQueue subMerchantProvisioningQueue)
    {
        _businessRepository = businessRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _subMerchantProvisioningQueue = subMerchantProvisioningQueue;
    }

    public async Task<Result<CreateBusinessResponse>> Handle(
        CreateBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var businessName = BusinessName.Create(request.Name);

        if (await _businessRepository.ExistsAsync(
                businessName,
                cancellationToken: cancellationToken))
        {
            return Result<CreateBusinessResponse>.Failure(
                BusinessErrors.AlreadyExists);
        }

        var businessType =
            Enumeration.FromValue<BusinessType>(request.Type);

        var merchantType =
            Enumeration.FromValue<MerchantType>(
                request.MerchantType);

        var merchantDetailsResult =
            BusinessMerchantDetails.Create(
                merchantType,
                request.LegalName,
                request.IdentityNumber,
                request.TaxNumber,
                request.TaxOffice,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.City,
                request.Country,
                request.Iban);

        if (merchantDetailsResult.IsFailure)
        {
            return Result<CreateBusinessResponse>.Failure(
                merchantDetailsResult.Error);
        }

        var merchantDetails =
            merchantDetailsResult.Value;

        if (merchantDetails is null)
        {
            return Result<CreateBusinessResponse>.Failure(
                new Error(
                    "Business.InvalidMerchantDetails",
                    "Merchant details could not be created."));
        }

        var ownerId = _currentUser.UserId;

        if (ownerId is null)
        {
            return Result<CreateBusinessResponse>.Failure(
                BusinessErrors.Unauthorized);
        }

        var result = Business.Create(
            businessName,
            businessType,
            merchantDetails,
            ownerId);

        if (result.IsFailure)
        {
            return Result<CreateBusinessResponse>.Failure(
                result.Error);
        }

        var business = result.Value;

        if (business is null)
        {
            return Result<CreateBusinessResponse>.Failure(
                BusinessErrors.UserNotAuthenticated);
        }

        await _businessRepository.AddAsync(
            business,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _subMerchantProvisioningQueue.QueueAsync(
            business.Id.Value,
            cancellationToken);

        return Result<CreateBusinessResponse>.Success(
            new CreateBusinessResponse(
                business.Id.Value,
                business.Name.Value,
                business.Type.Id,
                business.Type.Name));
    }
}

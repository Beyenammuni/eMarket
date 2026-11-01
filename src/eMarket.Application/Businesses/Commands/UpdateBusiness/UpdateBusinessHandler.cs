
using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.UpdateBusiness;

internal sealed class UpdateBusinessHandler
    : IRequestHandler<UpdateBusinessCommand, Result<UpdateBusinessResponse>>
{
    private readonly IBusinessRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBusinessAuthorization _authorization;
    private readonly ISensitiveDataProtector _sensitiveDataProtector;

    public UpdateBusinessHandler(
        IBusinessRepository repository,
        IUnitOfWork unitOfWork,
        IBusinessAuthorization authorization,
        ISensitiveDataProtector sensitiveDataProtector)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _sensitiveDataProtector = sensitiveDataProtector;
    }

    public async Task<Result<UpdateBusinessResponse>> Handle(
        UpdateBusinessCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        var hasPermission = await _authorization.HasPermissionAsync(
            businessId,
            Permissions.Business.Update,
            cancellationToken);

        if (!hasPermission)
        {
            return Result<UpdateBusinessResponse>.Failure(
                BusinessErrors.Forbidden);
        }

        var business = await _repository.GetByIdAsync(
            businessId,
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

        var contactResult = business.UpdateContactDetails(
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.Country);

        if (contactResult.IsFailure)
        {
            return Result<UpdateBusinessResponse>.Failure(
                contactResult.Error);
        }

        var identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber)
            ? business.MerchantDetails.IdentityNumber
            : _sensitiveDataProtector.Protect(
                request.IdentityNumber.Trim());

        var representativeIdentityNumber =
            string.IsNullOrWhiteSpace(request.RepresentativeIdentityNumber)
                ? business.MerchantDetails.RepresentativeIdentityNumber
                : _sensitiveDataProtector.Protect(
                    request.RepresentativeIdentityNumber.Trim());

        var legalResult = business.UpdateLegalDetails(
            request.LegalName,
            identityNumber,
            request.TaxNumber,
            request.TaxOffice,
            request.RepresentativeFullName,
            representativeIdentityNumber);

        if (legalResult.IsFailure)
        {
            return Result<UpdateBusinessResponse>.Failure(
                legalResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateBusinessResponse>.Success(
            new UpdateBusinessResponse(
                business.Id.Value,
                business.Name.Value,
                business.Type.Name,
                business.MerchantDetails.LegalName,
                business.MerchantDetails.TaxNumber,
                business.MerchantDetails.TaxOffice));
    }
}

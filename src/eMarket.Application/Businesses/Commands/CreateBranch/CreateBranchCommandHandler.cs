using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.CreateBranch;

public sealed class CreateBranchCommandHandler
    : IRequestHandler<CreateBranchCommand, Result<Guid>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessAuthorization _businessAuthorization;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(
        IBusinessRepository businessRepository,
        IBusinessAuthorization businessAuthorization,
        ISellerPlanFeatureService sellerPlanFeatureService,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _businessAuthorization = businessAuthorization;
        _sellerPlanFeatureService = sellerPlanFeatureService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        var hasPermission =
            await _businessAuthorization.HasPermissionAsync(
                businessId,
                Permissions.Branches.Manage,
                cancellationToken);

        if (!hasPermission)
        {
            return Result<Guid>.Failure(
                new Error("Branch.Forbidden", "You are not allowed to manage branches."));
        }

        var hasFeature = await _sellerPlanFeatureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Branches.Management,
            cancellationToken);

        if (!hasFeature)
        {
            return Result<Guid>.Failure(
                new Error(
                    "Branch.FeatureNotAvailable",
                    "Branch management requires a PRO subscription."));
        }

        var business = await _businessRepository.GetWithBranchesAsync(
            businessId,
            cancellationToken);

        if (business is null)
        {
            return Result<Guid>.Failure(
                new Error("Business.NotFound", "Business was not found."));
        }

        var branchResult = Branch.Create(
            businessId,
            request.Name,
            request.Address,
            request.City,
            request.District,
            request.PhoneNumber,
            request.Latitude,
            request.Longitude,
            isMain: false);

        if (branchResult.IsFailure)
            return Result<Guid>.Failure(branchResult.Error);

        var addResult = business.AddBranch(branchResult.Value);

        if (addResult.IsFailure)
            return Result<Guid>.Failure(addResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(branchResult.Value.Id.Value);
    }
}

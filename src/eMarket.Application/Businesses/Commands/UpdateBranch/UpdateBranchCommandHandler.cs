using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.UpdateBranch;

internal sealed class UpdateBranchCommandHandler
    : IRequestHandler<UpdateBranchCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessAuthorization _businessAuthorization;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchCommandHandler(
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

    public async Task<Result> Handle(
        UpdateBranchCommand request,
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
            return Result.Failure(
                new Error("Branch.Forbidden", "You are not allowed to manage branches."));
        }

        var hasFeature = await _sellerPlanFeatureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Branches.Management,
            cancellationToken);

        if (!hasFeature)
        {
            return Result.Failure(
                new Error(
                    "Branch.FeatureNotAvailable",
                    "Branch management requires a PRO subscription."));
        }

        var business = await _businessRepository.GetWithBranchesAsync(
            businessId,
            cancellationToken);

        if (business is null)
            return Result.Failure(BusinessErrors.NotFound);

        var branch = business.Branches.FirstOrDefault(
            x => x.Id == BranchId.Create(request.BranchId));

        if (branch is null)
        {
            return Result.Failure(
                new Error("Branch.NotFound", "Branch was not found."));
        }

        var updateResult = branch.UpdateDetails(
            request.Name,
            request.Address,
            request.City,
            request.District,
            request.PhoneNumber,
            request.Latitude,
            request.Longitude);

        if (updateResult.IsFailure)
            return updateResult;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

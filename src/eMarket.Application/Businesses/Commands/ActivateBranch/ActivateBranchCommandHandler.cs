using eMarket.Application.Common.Constants;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using eMarket.Application.Common.Authorization;

namespace eMarket.Application.Businesses.Branches.Commands.ActivateBranch;

public sealed class ActivateBranchCommandHandler
    : IRequestHandler<ActivateBranchCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessAuthorization _authorization;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateBranchCommandHandler(
        IBusinessRepository businessRepository,
        IBusinessAuthorization authorization,
        ISellerPlanFeatureService sellerPlanFeatureService,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _authorization = authorization;
        _sellerPlanFeatureService = sellerPlanFeatureService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ActivateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        var hasPermission = await _authorization.HasPermissionAsync(
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
                new Error("Branch.FeatureNotAvailable", "This feature requires a PRO subscription."));
        }

        var business = await _businessRepository.GetWithBranchesAsync(
            businessId,
            cancellationToken);

        if (business is null)
            return Result.Failure(BusinessErrors.NotFound);

        var branchId = BranchId.Create(request.BranchId);

        var branch = business.Branches
            .FirstOrDefault(x => x.Id == branchId);

        if (branch is null)
        {
            return Result.Failure(
                new Error("Branch.NotFound", "Branch not found."));
        }

        business.ActivateBranch(branch.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

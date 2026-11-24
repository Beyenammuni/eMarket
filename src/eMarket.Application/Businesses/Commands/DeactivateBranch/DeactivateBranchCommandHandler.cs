using eMarket.Application.Common.Constants;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using eMarket.Application.Common.Authorization;

namespace eMarket.Application.Businesses.Branches.Commands.DeactivateBranch;

public sealed class DeactivateBranchCommandHandler
    : IRequestHandler<DeactivateBranchCommand, Result>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IBusinessAuthorization _authorization;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchCommandHandler(
        IBusinessRepository businessRepository,
        IOrderRepository orderRepository,
        IBusinessAuthorization authorization,
        ISellerPlanFeatureService sellerPlanFeatureService,
        IUnitOfWork unitOfWork)
    {
        _businessRepository = businessRepository;
        _orderRepository = orderRepository;
        _authorization = authorization;
        _sellerPlanFeatureService = sellerPlanFeatureService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeactivateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var hasPermission = await _authorization.HasPermissionAsync(
     BusinessId.Create(request.BusinessId),
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
            new BusinessId(request.BusinessId),
            cancellationToken);

        if (business is null)
            return Result.Failure(BusinessErrors.NotFound);

        var branch = business.Branches
            .FirstOrDefault(x => x.Id == BranchId.Create(request.BranchId));

        if (branch is null)
        {
            return Result.Failure(
                new Error("Branch.NotFound", "Branch not found."));
        }

        if (branch.IsMain)
        {
            return Result.Failure(
                new Error("Branch.MainCannotBeDeactivated", "The main branch cannot be deactivated."));
        }

        var hasActiveOrders = await _orderRepository.HasActiveOrdersAsync(
            request.BranchId,
            cancellationToken);

        if (hasActiveOrders)
        {
            return Result.Failure(
                new Error(
                    "Branch.HasActiveOrders",
                    "The branch cannot be deactivated while it has pending or active orders."));
        }

        var result = business.DeactivateBranch(branch.Id);

        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetBusinessActivityLogs;

internal sealed class GetBusinessActivityLogsHandler
    : IRequestHandler<
        GetBusinessActivityLogsQuery,
        Result<GetBusinessActivityLogsResponse>>
{
    private readonly IBusinessDbContext _context;
    private readonly IBusinessRepository _businessRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;

    public GetBusinessActivityLogsHandler(
        IBusinessDbContext context,
        IBusinessRepository businessRepository,
        ICurrentUser currentUser,
        ISellerPlanFeatureService sellerPlanFeatureService)
    {
        _context = context;
        _businessRepository = businessRepository;
        _currentUser = currentUser;
        _sellerPlanFeatureService = sellerPlanFeatureService;
    }

    public async Task<Result<GetBusinessActivityLogsResponse>> Handle(
        GetBusinessActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.UserId;

        if (currentUserId is null)
        {
            return Result<GetBusinessActivityLogsResponse>.Failure(
                BusinessErrors.UserNotAuthenticated);
        }

        var businessId = BusinessId.Create(request.BusinessId);

        var currentMember = await _businessRepository.GetMemberAsync(
            businessId,
            currentUserId,
            cancellationToken);

        if (currentMember is null ||
            currentMember.Role != BusinessRole.Owner)
        {
            return Result<GetBusinessActivityLogsResponse>.Failure(
                BusinessErrors.NotOwner);
        }

        var hasFeature = await _sellerPlanFeatureService.HasFeatureAsync(
            request.BusinessId,
            SellerPlanFeatureCodes.Staff.AdvancedManagement,
            cancellationToken);

        if (!hasFeature)
        {
            return Result<GetBusinessActivityLogsResponse>.Failure(
                BusinessErrors.Forbidden);
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var logsQuery = _context.BusinessActivityLogs
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        var totalCount = await logsQuery.CountAsync(cancellationToken);

        var items = await logsQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BusinessActivityLogItem(
                x.Id,
                x.ActorUserId.Value,
                x.TargetUserId == null ? null : x.TargetUserId.Value,
                x.Action,
                x.Description,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<GetBusinessActivityLogsResponse>.Success(
            new GetBusinessActivityLogsResponse(
                items,
                page,
                pageSize,
                totalCount));
    }
}

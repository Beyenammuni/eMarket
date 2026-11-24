using eMarket.Application.Common.Constants;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetBusinessBranches;

internal sealed class GetBusinessBranchesHandler
    : IRequestHandler<
        GetBusinessBranchesQuery,
        Result<IReadOnlyCollection<GetBusinessBranchesResponse>>>
{
    private readonly IBusinessDbContext _context;
    private readonly ISellerPlanFeatureService _sellerPlanFeatureService;
    public GetBusinessBranchesHandler(IBusinessDbContext context, ISellerPlanFeatureService sellerPlanFeatureService)
    {
        _context = context;
        _sellerPlanFeatureService = sellerPlanFeatureService;
    }

    public async Task<Result<IReadOnlyCollection<GetBusinessBranchesResponse>>> Handle(
        GetBusinessBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var businessId = BusinessId.Create(request.BusinessId);

        var businessExists = await _context.Businesses
            .AsNoTracking()
            .AnyAsync(x => x.Id == businessId, cancellationToken);

        if (!businessExists)
        {
            return Result<IReadOnlyCollection<GetBusinessBranchesResponse>>
                .Failure(BusinessErrors.NotFound);
        }
        var hasFeature = await _sellerPlanFeatureService.HasFeatureAsync(
    request.BusinessId,
    SellerPlanFeatureCodes.Branches.Management,
    cancellationToken);

        var branches = await _context.Branches
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .Where(x => hasFeature || x.IsMain)
            .OrderByDescending(x => x.IsMain)
            .ThenBy(x => x.Name)
            .Select(x => new GetBusinessBranchesResponse(
                x.Id.Value,
                x.Name,
                x.Address,                    
                x.City,
                x.District,
                x.PhoneNumber,
                x.Latitude,
                x.Longitude,
                x.IsMain,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<GetBusinessBranchesResponse>>
            .Success(branches);
    }
}

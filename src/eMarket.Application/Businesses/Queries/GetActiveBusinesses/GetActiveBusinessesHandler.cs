using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetActiveBusinesses;

internal sealed class GetActiveBusinessesHandler
    : IRequestHandler<
        GetActiveBusinessesQuery,
        Result<IReadOnlyList<GetActiveBusinessesResponse>>>
{
    private readonly IBusinessDbContext _context;

    public GetActiveBusinessesHandler(
        IBusinessDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<GetActiveBusinessesResponse>>> Handle(
        GetActiveBusinessesQuery request,
        CancellationToken cancellationToken)
    {
        var businesses = await _context.Businesses
            .AsNoTracking()
            .Where(b => b.Status == BusinessStatus.Active)
            .Select(b => new GetActiveBusinessesResponse(
                b.Id.Value,
                b.Name.Value,
                b.Type.Id,
                b.Type.Name,
                b.Status.ToString(),
                b.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetActiveBusinessesResponse>>
            .Success(businesses);
    }
}

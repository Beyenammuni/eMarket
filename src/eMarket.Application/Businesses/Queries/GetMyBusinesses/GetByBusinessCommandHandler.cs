using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetMyBusinesses;

internal sealed class GetMyBusinessesHandler
    : IRequestHandler<
        GetMyBusinessesQuery,
        Result<IReadOnlyList<GetMyBusinessesResponse>>>
{
    private readonly IBusinessDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetMyBusinessesHandler(
        IBusinessDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<GetMyBusinessesResponse>>> Handle(
        GetMyBusinessesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var businesses = await _context.Businesses
            .AsNoTracking()
            .Where(b => b.Members.Any(m =>
                m.UserId == userId &&
                m.IsActive))
            .Select(b => new GetMyBusinessesResponse(
                b.Id.Value,
                b.Name.Value,
                b.Type.Id,
                b.Type.Name,
                b.Status.ToString(),
                b.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetMyBusinessesResponse>>.Success(businesses);
    }
}

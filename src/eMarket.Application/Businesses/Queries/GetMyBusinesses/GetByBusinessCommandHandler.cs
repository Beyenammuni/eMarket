using eMarket.Application.Businesses.Commands.AddBusinessMember;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace eMarket.Application.Businesses.Queries.GetMyBusinesses;

internal sealed class GetMyBusinessesHandler
    : IRequestHandler<
        GetMyBusinessesQuery,
        Result<IReadOnlyList<GetMyBusinessesResponse>>>
{
    private readonly IBusinessDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IBusinessAuthorization _authorization;

    public GetMyBusinessesHandler(
        IBusinessDbContext context,
        ICurrentUser currentUser,
        IBusinessAuthorization authorization)
    {
        _context = context;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<Result<IReadOnlyList<GetMyBusinessesResponse>>> Handle(
        GetMyBusinessesQuery request,
        CancellationToken cancellationToken)
    {
        
        var userId = _currentUser.UserId;

        var businesses = await _context.Businesses
            .AsNoTracking()
            .Where(x => x.Members.Any(m => m.UserId == userId))
            .Include(b => b.Members)
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

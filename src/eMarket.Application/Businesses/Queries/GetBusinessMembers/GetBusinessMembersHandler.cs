using eMarket.Application.Common.Interfaces;
using eMarket.Domain.Businesses;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetBusinessMembers;

internal sealed class GetBusinessMembersHandler
    : IRequestHandler<
        GetBusinessMembersQuery,
        Result<IReadOnlyList<GetBusinessMembersResponse>>>
{
    private readonly IBusinessDbContext _context;

    public GetBusinessMembersHandler(
        IBusinessDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<GetBusinessMembersResponse>>> Handle(
        GetBusinessMembersQuery request,
        CancellationToken cancellationToken)
    {
        var members = await _context.Businesses
            .AsNoTracking()
            .Where(x => x.Id == BusinessId.Create(request.BusinessId))
            .SelectMany(x => x.Members)
            .Select(x => new GetBusinessMembersResponse(
                x.UserId.Value,
                x.Role.Name,
                x.IsActive,
                x.JoinedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetBusinessMembersResponse>>.Success(members);
    }
}

using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Queries.GetBusinessMembers;

public sealed record GetBusinessMembersQuery(Guid BusinessId)
    : IRequest<Result<IReadOnlyList<GetBusinessMembersResponse>>>;

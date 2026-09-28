using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetAvailableBusinessMembers;

public sealed record GetAvailableBusinessMembersQuery(
    Guid BusinessId
) : IRequest<Result<List<GetAvailableBusinessMemberResponse>>>;

public sealed record GetAvailableBusinessMemberResponse(
    Guid Id,
    string FullName,
    string Email,
    string Username
);

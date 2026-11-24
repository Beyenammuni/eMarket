using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Queries.GetBusinessBranches;

public sealed record GetBusinessBranchesQuery(
    Guid BusinessId
) : IRequest<Result<IReadOnlyCollection<GetBusinessBranchesResponse>>>;

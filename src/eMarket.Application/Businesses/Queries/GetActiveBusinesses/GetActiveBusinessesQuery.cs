using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Queries.GetActiveBusinesses;

public sealed record GetActiveBusinessesQuery()
    : IRequest<Result<IReadOnlyList<GetActiveBusinessesResponse>>>;

using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Queries.GetMyBusinesses;

public sealed record GetMyBusinessesQuery
    : IRequest<Result<IReadOnlyList<GetMyBusinessesResponse>>>;

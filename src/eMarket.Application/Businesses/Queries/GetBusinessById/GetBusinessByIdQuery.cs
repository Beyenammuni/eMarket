using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Queries.GetBusinessById;

public sealed record GetBusinessByIdQuery(Guid Id)
    : IRequest<Result<GetBusinessByIdResponse>>;

using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id)
    : IRequest<Result<GetProductByIdResponse>>;

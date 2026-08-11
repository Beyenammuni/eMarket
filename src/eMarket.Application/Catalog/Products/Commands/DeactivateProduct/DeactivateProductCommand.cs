using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.DeactivateProduct;

public sealed record DeactivateProductCommand(
    Guid Id)
    : IRequest<Result>;

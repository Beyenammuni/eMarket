using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.ActivateProduct;

public sealed record ActivateProductCommand(
    Guid Id)
    : IRequest<Result>;

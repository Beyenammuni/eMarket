using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddStock;

public sealed record AddStockCommand(
    Guid Id,
    Guid BusinessId,
    int Quantity)
    : IRequest<Result>;

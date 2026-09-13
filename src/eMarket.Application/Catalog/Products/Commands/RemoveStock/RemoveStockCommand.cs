using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.RemoveStock;

public sealed record RemoveStockCommand(
    Guid Id,
    Guid BusniessId,

    int Quantity)
    : IRequest<Result>;

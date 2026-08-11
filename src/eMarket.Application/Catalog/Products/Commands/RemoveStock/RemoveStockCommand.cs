using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.RemoveStock;

public sealed record RemoveStockCommand(
    Guid Id,
    int Quantity)
    : IRequest<Result>;

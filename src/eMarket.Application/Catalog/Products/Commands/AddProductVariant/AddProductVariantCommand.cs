
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddProductVariant;

public sealed record AddProductVariantCommand(
    Guid ProductId,
    string Name,
    string? Sku,
    decimal Price,
    int StockQuantity)
    : IRequest<Result<Guid>>;

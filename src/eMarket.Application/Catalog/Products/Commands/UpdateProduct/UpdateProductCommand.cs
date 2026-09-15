using eMarket.Domain.Common;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Sku,
    Guid CategoryId,
    string? ImageUrl,
    Currency Currency)
    : IRequest<Result<UpdateProductResponse>>;

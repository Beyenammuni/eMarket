using eMarket.SharedKernel.Results;
using MediatR;

using eMarket.Domain.Common;
namespace eMarket.Application.Catalog.Products.Commands.CreateProduct;


public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    Currency Currency,
    string Sku,
    Guid CategoryId,
    string? ImageUrl)
    : IRequest<Result<CreateProductResponse>>;


using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddProductOption;

public sealed record AddProductOptionCommand(
    Guid ProductId,
    Guid OptionGroupId,
    string Name,
    decimal AdditionalPrice,
    int DisplayOrder)
    : IRequest<Result<Guid>>;

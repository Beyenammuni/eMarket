
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Commands.AddProductOptionGroup;

public sealed record AddProductOptionGroupCommand(
    Guid ProductId,
    string Name,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    int DisplayOrder)
    : IRequest<Result<Guid>>;

using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Catalog.Categories.Commands.DeactiveCategory;

public sealed record DeactivateCategoryCommand(Guid Id)
    : IRequest<Result>;

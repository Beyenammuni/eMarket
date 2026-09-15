using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Catalog.Categories.Commands.DeactivateCategory;

public sealed record DeactivateCategoryCommand(Guid Id)
    : IRequest<Result>;

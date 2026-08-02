

using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Categories.Commands.ActivateCategory
{
    public sealed record ActivateCategoryCommand(Guid Id)
      : IRequest<Result>;
}

using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Categories.Commands.UpdateCategory
{
    public sealed record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Result<UpdateCategoryResponse>>;
}

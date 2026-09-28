using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Categories.Commands.CreateCategory
{
    public sealed record CreateCategoryCommand(
        string Name,
        Guid? ParentCategoryId
    ) : IRequest<Result<CreateCategoryResponse>>;
}

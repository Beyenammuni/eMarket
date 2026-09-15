using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Categories.Commands.CreateCategory
{
    public sealed record CreateCategoryResponse(
        Guid Id,
        string Name,
        string Status,
        DateTime CreatedAt);
}

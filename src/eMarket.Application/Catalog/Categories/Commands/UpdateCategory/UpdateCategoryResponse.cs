using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Categories.Commands.UpdateCategory
{
    public sealed record UpdateCategoryResponse(
        Guid Id,
        string Name,
        string Status);
}

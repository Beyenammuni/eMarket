using eMarket.Domain.Catalog.Categories;
using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Categories.Queries.GetCategoryById
{
    public sealed record GetCategories : IRequest<Result<List<CategoryResponse>>>;
}

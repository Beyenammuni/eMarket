using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Products.Queries.GetProducts
{
    public sealed class GetProductsQueryValidator
     : AbstractValidator<GetProductsQuery>
    {
        public GetProductsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.SortBy)
                .Must(value =>
                    string.IsNullOrWhiteSpace(value) ||
                    value.Equals("name", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("price", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
                .WithMessage(
                    "SortBy must be name, price, or createdAt.");
        }
    }
}

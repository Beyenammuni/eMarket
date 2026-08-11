using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace eMarket.Application.Catalog.Products.Queries.GetProductById
{
    public sealed class GetProductByIdValidator: AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdValidator()
        {
        RuleFor(x => x.Id).NotEmpty();
        }

    }
}

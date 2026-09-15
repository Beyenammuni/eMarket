using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Catalog.Products.Commands.DeleteProduct
{
    public sealed record DeleteProductCommand(
        Guid Id)
        : IRequest<Result>;
}

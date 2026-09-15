using eMarket.Application.Catalog.Products.Queries.GetProducts;
using eMarket.SharedKernel.Results;
 using MediatR;

    namespace eMarket.Application.Catalog.Products.Queries.GetProducts;

    public sealed record GetProductsQuery(
        int Page = 1,
        int PageSize = 10,
        string? Search = null,
        Guid? BusinessId = null,
        Guid? CategoryId = null,
        string? SortBy = null,
        bool SortDescending = false)
        : IRequest<Result<GetProductsResponse>>;

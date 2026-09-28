using eMarket.Application.Catalog.Products.Queries.GetProductById;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Categories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Queries.GetProducts;

internal sealed class GetProductsHandler
    : IRequestHandler<GetProductsQuery, Result<GetProductsResponse>>
{
    private readonly IProductRepository _repository;

    public GetProductsHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetProductsResponse>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (products, totalCount) =
        await _repository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.BusinessId is null
                ? null
                : BusinessId.Create(request.BusinessId.Value),
            request.CategoryId is null
                ? null
                : CategoryId.Create(request.CategoryId.Value),
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var items = products
            .Select(product => new ProductDto(
                product.Id.Value,
                product.BusinessId.Value,
                product.CategoryId.Value,
                product.Name.Value,
                product.Price.Amount,
                product.ImageUrl,
                product.StockQuantity,
                product.Status.ToString()))
            .ToList();

        return Result<GetProductsResponse>.Success(
            new GetProductsResponse(
                items,
                request.Page,
                request.PageSize,
                totalCount));
    }
}

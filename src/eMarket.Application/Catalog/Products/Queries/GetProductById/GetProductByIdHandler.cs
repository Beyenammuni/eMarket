using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Catalog.Products.Queries.GetProductById;

internal sealed class GetProductByIdHandler
    : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    private readonly IProductRepository _repository;

    public GetProductByIdHandler(
        IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetProductByIdResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            ProductId.Create(request.Id),
            cancellationToken);

        if (product is null)
        {
            return Result<GetProductByIdResponse>
                .Failure(ProductErrors.NotFound);
        }


        return Result<GetProductByIdResponse>.Success(
            new GetProductByIdResponse(
                product.Id.Value,
                product.BusinessId.Value,
                product.CategoryId.Value,
                product.Name.Value,
                product.Description.Value,
                product.Price.Amount,
                product.Sku.Value,
                product.ImageUrl,
                product.StockQuantity,
                product.Status.ToString(),
                product.CreatedAt,

                product.Variants
                    .Select(v => new ProductVariantResponse(
                        v.Id,
                        v.Name,
                        v.Sku,
                        v.Price,
                        v.StockQuantity,
                        v.IsActive))
                    .ToList(),

                product.OptionGroups
                    .Select(g => new ProductOptionGroupResponse(
                        g.Id,
                        g.Name,
                        g.IsRequired,
                        g.MinSelections,
                        g.MaxSelections,
                        g.DisplayOrder,
                        g.Options
                            .Select(o => new ProductOptionResponse(
                                o.Id,
                                o.Name,
                                o.AdditionalPrice,
                                o.IsActive,
                                o.DisplayOrder))
                            .ToList()))
                    .ToList()));

    }
}

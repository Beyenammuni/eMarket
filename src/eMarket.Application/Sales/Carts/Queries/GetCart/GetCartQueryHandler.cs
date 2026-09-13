using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Sales.Carts;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Queries.GetCart;

internal sealed class GetCartQueryHandler
    : IRequestHandler<GetCartQuery, Result<GetCartResponse>>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICurrentUser _currentUser;

    public GetCartQueryHandler(
        ICartRepository cartRepository,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<GetCartResponse>> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var cart = await _cartRepository.GetActiveByUserAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            return Result<GetCartResponse>.Failure(
                CartErrors.EmptyCart);
        }

        var items = cart.Items
            .Select(item =>
                new CartItemResponse(
                    item.ProductId.Value,
                    item.UnitPrice.Amount,
                    item.UnitPrice.Currency,
                    item.Quantity.Value,
                    item.TotalPrice))
            .ToList();

        var response = new GetCartResponse(
            cart.Id.Value,
            cart.TotalItems,
            cart.TotalAmount,
            items);

        return Result<GetCartResponse>.Success(response);
    }
}

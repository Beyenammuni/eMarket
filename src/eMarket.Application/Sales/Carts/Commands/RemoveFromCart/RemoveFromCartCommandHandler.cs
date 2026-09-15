using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Sales.Carts;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.RemoveFromCart;

internal sealed class RemoveFromCartCommandHandler
    : IRequestHandler<RemoveFromCartCommand, Result>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public RemoveFromCartCommandHandler(
        ICartRepository cartRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        RemoveFromCartCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetActiveByUserAsync(
            _currentUser.UserId,
            cancellationToken);

        if (cart is null)
        {
            return Result.Failure(
                CartErrors.EmptyCart);
        }

        var result = cart.RemoveItem(
            ProductId.Create(request.ProductId));

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}

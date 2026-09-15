using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Carts.Entities;
using eMarket.Domain.Sales.Carts.Events;
using eMarket.Domain.Sales.Carts.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Sales.Carts;

public sealed class Cart : AggregateRoot<CartId>
{
    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    private Cart(
        CartId id,
        UserId userId)
    {
        Id = id;
        UserId = userId;
        Status = CartStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public CartId Id { get; private set; } = default!;

    public UserId UserId { get; private set; } = default!;

    public CartStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<CartItem> Items =>
        _items.AsReadOnly();

    public decimal TotalAmount =>
        _items.Sum(x => x.TotalPrice);

    public int TotalItems =>
        _items.Sum(x => x.Quantity.Value);

    public static Result<Cart> Create(UserId userId)
    {
        var cart = new Cart(
            CartId.New(),
            userId);
        cart.AddDomainEvent(new CartCreatedDomainEvent(cart.Id));
        return Result<Cart>.Success(cart);
    }
    public Result Clear()
    {
        if (Status != CartStatus.Active)
        {
            return Result.Failure(
                CartErrors.AlreadyCheckedOut);
        }

        if (!_items.Any())
        {
            return Result.Failure(
                CartErrors.EmptyCart);
        }

        _items.Clear();

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CartClearedDomainEvent(Id));
        return Result.Success();
    }
    public Result AddItem(
     ProductId productId,
     Money unitPrice,
     Quantity quantity,
     int availableStock)
    {
        if (Status != CartStatus.Active)
        {
            return Result.Failure(
                CartErrors.AlreadyCheckedOut);
        }

        var existingItem = _items.FirstOrDefault(
            x => x.ProductId == productId);

        if (existingItem is not null)
        {
            var newQuantity =
                existingItem.Quantity.Value + quantity.Value;

            if (newQuantity > availableStock)
            {
                return Result.Failure(
                    CartErrors.InsufficientStock);
            }

            existingItem.IncreaseQuantity(
                quantity.Value);

            existingItem.UpdatePrice(unitPrice);

            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        if (quantity.Value > availableStock)
        {
            return Result.Failure(
                CartErrors.InsufficientStock);
        }

        _items.Add(
            new CartItem(
                Id,
                productId,
                unitPrice,
                quantity));

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CartItemAddedDomainEvent(Id, productId));

        return Result.Success();
    }


    public Result RemoveItem(ProductId productId)
    {
        if (Status != CartStatus.Active)
        {
            return Result.Failure(
                CartErrors.AlreadyCheckedOut);
        }

        var item = _items.FirstOrDefault(
            x => x.ProductId == productId);

        if (item is null)
        {
            return Result.Failure(
                CartErrors.ProductNotFound);
        }

        _items.Remove(item);

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CartItemRemovedDomainEvent(Id, productId));


        return Result.Success();
    }

    public Result ChangeItemQuantity(
        ProductId productId,
        Quantity quantity)
    {
        if (Status != CartStatus.Active)
        {
            return Result.Failure(
                CartErrors.AlreadyCheckedOut);
        }

        var item = _items.FirstOrDefault(
            x => x.ProductId == productId);

        if (item is null)
        {
            return Result.Failure(
                CartErrors.ProductNotFound);
        }

        item.ChangeQuantity(quantity);

        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CartItemQuantityChangedDomainEvent(Id, productId, quantity));


        return Result.Success();
    }



    public Result Checkout()
    {
        if (Status == CartStatus.CheckedOut)
        {
            return Result.Failure(
                CartErrors.AlreadyCheckedOut);
        }

        if (_items.Count == 0)
        {
            return Result.Failure(
                CartErrors.EmptyCart);
        }

        Status = CartStatus.CheckedOut;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}

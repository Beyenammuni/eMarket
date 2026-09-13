using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Common;

namespace eMarket.Domain.Subscriptions.Entities;

public sealed class SubscriptionItem : Entity<Guid>
{
    private SubscriptionItem() { }

    public SubscriptionItem(ProductId productId, int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
    }

    public ProductId ProductId { get; private set; } = default!;
    public int Quantity { get; private set; }

    public void ChangeQuantity(int quantity) => Quantity = quantity;
}

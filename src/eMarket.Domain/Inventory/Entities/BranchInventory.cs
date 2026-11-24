
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;

namespace eMarket.Domain.Inventory.Entities;

public sealed class BranchInventory
{
    private BranchInventory() { }

    private BranchInventory(
        Guid id,
        BranchId branchId,
        ProductId productId,
        Guid? variantId,
        int quantity)
    {
        Id = id;
        BranchId = branchId;
        ProductId = productId;
        VariantId = variantId;
        Quantity = quantity;
        ReservedQuantity = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public BranchId BranchId { get; private set; } = default!;

    public ProductId ProductId { get; private set; } = default!;

    public Guid? VariantId { get; private set; }

    // Physical quantity in the branch.
    public int Quantity { get; private set; }

    // Quantity reserved for pending orders.
    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;

    public DateTime UpdatedAt { get; private set; }

    public static BranchInventory Create(
        BranchId branchId,
        ProductId productId,
        Guid? variantId,
        int quantity)
    {
        ArgumentNullException.ThrowIfNull(branchId);
        ArgumentNullException.ThrowIfNull(productId);

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        return new BranchInventory(
            Guid.NewGuid(),
            branchId,
            productId,
            variantId,
            quantity);
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity = checked(Quantity + quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                "Insufficient available stock.");

        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                "Insufficient available stock.");

        ReservedQuantity = checked(ReservedQuantity + quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                "Cannot release more stock than reserved.");

        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void FulfillReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                "Insufficient reserved stock.");

        Quantity -= quantity;
        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdjustStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity));

        if (newQuantity < ReservedQuantity)
            throw new InvalidOperationException(
                "Stock cannot be adjusted below reserved quantity.");

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

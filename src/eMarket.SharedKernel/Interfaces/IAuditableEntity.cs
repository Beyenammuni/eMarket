namespace eMarket.SharedKernel.DomainEvent
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; }
        DateTime? UpdatedAt { get; }
        DateTime? DeletedAt { get; }
    }
}
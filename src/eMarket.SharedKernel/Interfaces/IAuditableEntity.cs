namespace eMarket.SharedKernel.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; }
        DateTime? UpdatedAt { get; }
        DateTime? DeletedAt { get; }
    }
}
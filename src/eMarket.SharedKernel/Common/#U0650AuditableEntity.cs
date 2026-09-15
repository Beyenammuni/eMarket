using eMarket.SharedKernel.DomainEvent;

namespace  eMarket.SharedKernel.Common;

public class AuditableEntity<TKey> : Entity<TKey> 
where TKey : IEquatable<TKey>
{
    public AuditableEntity(List<IDomainEvent> domainEvents) : base() { }

    public DateTime CreatedOnUtc { get; protected set; }
    public string? CreatedBy { get; protected set; }

    public DateTime? LastModifiedOnUtc { get; protected set; }

    public string? LastModifiedBy { get; protected set; }

    public DateTime? DeletedOnUtc { get; protected set; }

    public string? DeletedBy { get; protected set; }

    public bool IsDeleted { get; protected set; }

    public void MarkAsCreated(string? user)
    {
        CreatedOnUtc = DateTime.UtcNow;
        CreatedBy = user;
    }
    public void MarkAsModified(string? user)
    {
        LastModifiedOnUtc = DateTime.UtcNow;
        LastModifiedBy = user;
    }
     public void MarkAsDeleted(string? user)
    {
        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;
        DeletedBy = user;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedOnUtc = null;
        DeletedBy = null;
    }
}

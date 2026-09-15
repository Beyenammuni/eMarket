using eMarket.SharedKernel.DomainEvent;

namespace eMarket.SharedKernel.Common
{
    /// <summary>
    /// Represents a base entity class that provides common properties and methods for entities in the domain.
    /// </summary>
    public abstract class Entity<Tkey> : IEntity<Tkey>, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public virtual Tkey Id {get; protected set; } = default!;

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        protected void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not Entity<Tkey> other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return EqualityComparer<Tkey>.Default.Equals(Id, other.Id);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), Id);
        }
        public static bool operator ==(Entity<Tkey> left, Entity<Tkey> right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }
        public static bool operator !=(Entity<Tkey> left, Entity<Tkey> right)
        {
            return !(left == right);
        }
    }
}

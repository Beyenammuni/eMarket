namespace eMarket.SharedKernel.DomainEvent
{
    public interface IEntity<IKey>
    {
        IKey Id { get; }
    }
}
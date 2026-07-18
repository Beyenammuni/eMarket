namespace eMarket.SharedKernel.Interfaces
{
    public abstract class DomainEvent : IDomainEvent
    {
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
        public DateTime OccurredOn {get;}
        }
}
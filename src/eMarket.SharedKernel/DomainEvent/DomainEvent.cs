namespace eMarket.SharedKernel.DomainEvent
{
    public abstract record DomainEvent : IDomainEvent
    {
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
        public DateTime OccurredOn {get;}
        }
}

namespace eMarket.SharedKernel.Interfaces
{
    public interface IEntity<IKey>
    {
        IKey Id { get; }
    }
}
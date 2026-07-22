using System;

namespace eMarket.SharedKernel.Interfaces
{
    /// <summary>
    /// Represents a domain event that can be published and handled within the domain layer.
    /// </summary>

    public interface IDomainEvent
    {
       DateTime OccurredOn { get; } 
    }   
}
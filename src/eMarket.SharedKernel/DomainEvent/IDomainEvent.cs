using System;
using MediatR;

namespace eMarket.SharedKernel.Interfaces
{
    /// <summary>
    /// Represents a domain event that can be published and handled within the domain layer.
    /// </summary>

    public interface IDomainEvent : INotification
    {
       DateTime OccurredOn { get; } 
    }   
}
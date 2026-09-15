using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Payments.Events;

public sealed record PaymentSucceededDomainEvent(
    PaymentId PaymentId,
    Guid OrderId)
    : DomainEvent;

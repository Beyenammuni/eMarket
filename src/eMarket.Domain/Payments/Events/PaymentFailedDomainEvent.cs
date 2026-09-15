using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Payments.Events;

public sealed record PaymentFailedDomainEvent(
    PaymentId PaymentId,
    Guid OrderId)
    : DomainEvent;

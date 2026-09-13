using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Payments.Events;

public sealed record PaymentCreatedDomainEvent(
    PaymentId PaymentId)
    : DomainEvent;

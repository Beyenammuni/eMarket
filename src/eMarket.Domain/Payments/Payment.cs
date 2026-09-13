using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Payments.Events;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Payments;

public sealed class Payment
    : AggregateRoot<PaymentId>
{
    private Payment()
    {
    }

    private Payment(
        PaymentId id,
        Guid orderId,
        Money amount,
        PaymentMethod method)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Method = method;

        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid OrderId { get; private set; }

    public Money Amount { get; private set; } = default!;

    public PaymentStatus Status { get; private set; }

    public PaymentMethod Method { get; private set; }

    public string? Provider { get; private set; }

    public string? ProviderPaymentId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public static Result<Payment> Create(
        Guid orderId,
        Money amount,
        PaymentMethod method)
    {
        if (amount.Amount <= 0)
        {
            return Result<Payment>.Failure(
                PaymentErrors.InvalidAmount);
        }

        var payment = new Payment(
            PaymentId.New(),
            orderId,
            amount,
            method);

        payment.AddDomainEvent(
            new PaymentCreatedDomainEvent(
                payment.Id));

        return Result<Payment>.Success(payment);
    }

    public Result MarkAsSucceeded(
        string provider,
        string providerPaymentId)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return Result.Success();
        }

        Status = PaymentStatus.Succeeded;
        Provider = provider;
        ProviderPaymentId = providerPaymentId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new PaymentSucceededDomainEvent(
                Id,
                OrderId));

        return Result.Success();
    }

    public Result MarkAsFailed()
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return Result.Failure(
                PaymentErrors.AlreadySucceeded);
        }

        Status = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new PaymentFailedDomainEvent(
                Id,
                OrderId));

        return Result.Success();
    }
}

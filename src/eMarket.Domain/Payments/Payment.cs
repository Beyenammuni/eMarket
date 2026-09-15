using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Payments.Events;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Payments;

public sealed class Payment
: AggregateRoot<PaymentId>
{
    private const decimal DefaultPlatformCommissionRate = 10m;

private Payment()
    {
    }

    private Payment(
        PaymentId id,
        Guid orderId,
        Money amount,
        PaymentMethod method,
        decimal platformCommissionRate)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Method = method;

        PlatformCommissionRate = platformCommissionRate;

        PlatformCommissionAmount =
            amount.Multiply(platformCommissionRate / 100m);

        SellerAmount =
            amount.Subtract(PlatformCommissionAmount);

        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid OrderId { get; private set; }

    public Money Amount { get; private set; } = default!;

    public decimal PlatformCommissionRate { get; private set; }

    public Money PlatformCommissionAmount { get; private set; } = default!;

    public Money SellerAmount { get; private set; } = default!;

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
        return Create(
            orderId,
            amount,
            method,
            DefaultPlatformCommissionRate);
    }

    public static Result<Payment> Create(
        Guid orderId,
        Money amount,
        PaymentMethod method,
        decimal platformCommissionRate)
    {
        if (amount.Amount <= 0)
        {
            return Result<Payment>.Failure(
                PaymentErrors.InvalidAmount);
        }

        if (platformCommissionRate < 0 ||
            platformCommissionRate > 100)
        {
            return Result<Payment>.Failure(
                new Error(
                    "Payment.InvalidCommissionRate",
                    "Platform commission rate must be between 0 and 100."));
        }

        var payment = new Payment(
            PaymentId.New(),
            orderId,
            amount,
            method,
            platformCommissionRate);

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

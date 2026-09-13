using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Payments;
using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace eMarket.Application.Sales.Payments.Commands.CreatePayment;

internal sealed class CreatePaymentCommandHandler
    : IRequestHandler<
        CreatePaymentCommand,
        Result<CreatePaymentResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;

    public CreatePaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _configuration = configuration;
    }

    public async Task<Result<CreatePaymentResponse>> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.ReturnUrl, UriKind.Absolute, out var returnUri) ||
            (returnUri.Scheme != Uri.UriSchemeHttps && returnUri.Scheme != Uri.UriSchemeHttp))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error("Payment.InvalidReturnUrl", "Return URL must be an absolute HTTP(S) URL."));   
        }

        var allowedHosts = _configuration.GetSection("Payments:AllowedReturnHosts").Get<string[]>() ?? Array.Empty<string>();
        if (allowedHosts.Length > 0 && !allowedHosts.Contains(returnUri.Host, StringComparer.OrdinalIgnoreCase))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error("Payment.InvalidReturnUrl", "Return URL host is not allowed."));
        }

        // Get Order
        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(request.OrderId),
            cancellationToken);

        if (order is null)
        {
            return Result<CreatePaymentResponse>.Failure(
                OrderErrors.NotFound);
        }

        // Security
        if (order.UserId != _currentUser.UserId)
        {
            return Result<CreatePaymentResponse>.Failure(
                OrderErrors.NotFound);
        }

        // Order must contain items
        if (!order.Items.Any())
        {
            return Result<CreatePaymentResponse>.Failure(
                OrderErrors.EmptyOrder);
        }

        // Check existing payment
        var existingPayment =
            await _paymentRepository.GetByOrderIdAsync(
                order.Id.Value,
                cancellationToken);

        if (existingPayment is not null)
        {
            return Result<CreatePaymentResponse>.Failure(
                PaymentErrors.AlreadyExists);
        }

        // Create Payment
        var currency = order.Items
            .First()
            .UnitPrice
            .Currency;

        var paymentResult = Payment.Create(
            order.Id.Value,
            Money.Create(
                order.TotalAmount,
                currency),
            PaymentMethod.Card);

        if (paymentResult.IsFailure)
        {
            return Result<CreatePaymentResponse>.Failure(
                paymentResult.Error);
        }

        var payment = paymentResult.Value;

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        // Create provider payment
        var gatewayResult =
            await _paymentGateway.CreatePaymentAsync(
                new PaymentGatewayRequest(
                    payment.Id.Value,
                    order.Id.Value,
                    payment.Amount.Amount,
                    payment.Amount.Currency.ToString(),
                    request.ReturnUrl),
                cancellationToken);

        if (!gatewayResult.IsSuccess)
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.ProviderFailed",
                    gatewayResult.Error ??
                    "Payment provider failed."));
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<CreatePaymentResponse>.Success(
            new CreatePaymentResponse(
                payment.Id.Value,
                order.Id.Value,
                payment.Amount.Amount,
                payment.Amount.Currency.ToString(),
                gatewayResult.PaymentUrl!));
    }
}

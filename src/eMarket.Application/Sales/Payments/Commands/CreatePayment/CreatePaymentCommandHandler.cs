using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
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
    private readonly IBusinessRepository _businessRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IConfiguration _configuration;

    public CreatePaymentCommandHandler(
        IOrderRepository orderRepository,
        IBusinessRepository businessRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _businessRepository = businessRepository;
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
        if (!Uri.TryCreate(
                request.ReturnUrl,
                UriKind.Absolute,
                out var returnUri) ||
            (returnUri.Scheme != Uri.UriSchemeHttps &&
             returnUri.Scheme != Uri.UriSchemeHttp))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.InvalidReturnUrl",
                    "Return URL must be an absolute HTTP(S) URL."));
        }

        var allowedHosts =
            _configuration
                .GetSection("Payments:AllowedReturnHosts")
                .Get<string[]>() ??
            Array.Empty<string>();

        if (allowedHosts.Length > 0 &&
            !allowedHosts.Contains(
                returnUri.Host,
                StringComparer.OrdinalIgnoreCase))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.InvalidReturnUrl",
                    "Return URL host is not allowed."));
        }

        var order = await _orderRepository.GetByIdAsync(
            OrderId.Create(request.OrderId),
            cancellationToken);

        if (order is null)
        {
            return Result<CreatePaymentResponse>.Failure(
                OrderErrors.NotFound);
        }

        if (order.UserId != _currentUser.UserId)
        {
            return Result<CreatePaymentResponse>.Failure(
                OrderErrors.NotFound);
        }

        if (!order.Items.Any())
        {
            return Result<CreatePaymentResponse>.Failure(
                OrderErrors.EmptyOrder);
        }

        var business = await _businessRepository.GetByIdAsync(
            order.BusinessId,
            cancellationToken);

        if (business is null)
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.BusinessNotFound",
                    "The business associated with the order was not found."));
        }

        if (string.IsNullOrWhiteSpace(
                business.IyzicoSubMerchantKey))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.SubMerchantNotConfigured",
                    "The business is not configured for marketplace payments."));
        }

        var existingPayment =
            await _paymentRepository.GetByOrderIdAsync(
                order.Id.Value,
                cancellationToken);

        if (existingPayment is not null)
        {
            return Result<CreatePaymentResponse>.Failure(
                PaymentErrors.AlreadyExists);
        }

        var currency = order.Items
            .First()
            .UnitPrice
            .Currency;

        var paymentResult = Payment.Create(
            order.Id.Value,
            Money.Create(
                order.TotalAmount,
                currency),
            PaymentMethod.Card,
            business.PlatformCommissionRate);

        if (paymentResult.IsFailure)
        {
            return Result<CreatePaymentResponse>.Failure(
                paymentResult.Error);
        }

        var payment = paymentResult.Value;


        if (payment is null)
        {
            return Result<CreatePaymentResponse>.Failure(
                PaymentErrors.NotFound);
        }

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);


        var fullName = order.DeliveryAddress.FullName.Trim();

        var nameParts = fullName
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        var buyerName = nameParts.Length > 1
            ? string.Join(
                ' ',
                nameParts.Take(nameParts.Length - 1))
            : nameParts[0];

        var buyerSurname = nameParts.Length > 1
            ? nameParts[^1]
            : nameParts[0];

        var buyer = new PaymentBuyer(
            _currentUser.UserId!.Value,
            buyerName,
            buyerSurname,
            _currentUser.Email ?? string.Empty,
            order.DeliveryAddress.PhoneNumber);

        var merchant = new PaymentGatewayMerchant(
            business.IyzicoSubMerchantKey,
            business.PlatformCommissionRate);

        var deliveryAddress = new PaymentGatewayAddress(
            order.DeliveryAddress.FullName,
            order.DeliveryAddress.PhoneNumber,
            order.DeliveryAddress.AddressLine,
            order.DeliveryAddress.City,
            order.DeliveryAddress.District,
            order.DeliveryAddress.PostalCode,
            order.DeliveryAddress.Neighborhood,
            order.DeliveryAddress.Street,
            order.DeliveryAddress.BuildingNumber,
            order.DeliveryAddress.ApartmentNumber,
            order.DeliveryAddress.Latitude,
            order.DeliveryAddress.Longitude);

        var items = order.Items
            .Select(item =>
                new PaymentGatewayItem(
                    item.ProductId.Value,
                    item.ProductName,
                    item.UnitPrice.Amount,
                    item.Quantity))
            .ToList();

        var gatewayResult =
            await _paymentGateway.CreatePaymentAsync(
                new PaymentGatewayRequest(
                    payment.Id.Value,
                    order.Id.Value,
                    payment.Amount.Amount,
                    payment.Amount.Currency.ToString(),
                    request.ReturnUrl,
                    buyer,
                    merchant,
                    deliveryAddress,
                    items),
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


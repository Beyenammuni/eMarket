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
    private readonly IUserRepository _userRepository;
    private readonly ICustomerIdentityProtector _identityProtector;
    public CreatePaymentCommandHandler(
        IOrderRepository orderRepository,
        IBusinessRepository businessRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IConfiguration configuration,
        IUserRepository userRepository,
        ICustomerIdentityProtector identityProtector)
    {
        _orderRepository = orderRepository;
        _businessRepository = businessRepository;
        _paymentRepository = paymentRepository;
        _paymentGateway = paymentGateway;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _configuration = configuration;
        _userRepository = userRepository;
        _identityProtector = identityProtector;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);


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

        var callbackUrl =
    $"{_configuration["Payments:OrderCallbackUrl"]}" +
    $"?paymentId={payment.Id.Value}";

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
        var user = await _userRepository.GetByIdAsync(
    _currentUser.UserId,
    cancellationToken);

        if (user is null ||
            string.IsNullOrWhiteSpace(user.EncryptedIdentityNumber))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.CustomerIdentityRequired",
                    "Please provide your identity number before payment."));
        }

        string identityNumber;

        try
        {
            identityNumber = _identityProtector.Unprotect(
                user.EncryptedIdentityNumber);
        }
        catch
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.CustomerIdentityUnavailable",
                    "Customer identity could not be verified."));
        }

        var buyer = new PaymentBuyer(
    _currentUser.UserId!.Value,
    buyerName,
    buyerSurname,
    _currentUser.Email ?? string.Empty,
    order.DeliveryAddress.PhoneNumber,
    identityNumber
);

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

        var gatewayResult = await _paymentGateway.CreatePaymentAsync(
    new PaymentGatewayRequest(
        payment.Id.Value,
        order.Id.Value,
        null,
        PaymentGatewayPaymentType.Order,
        payment.Amount.Amount,
        payment.Amount.Currency.ToString(),
        callbackUrl,
        buyer,
        merchant,
        deliveryAddress,
        items),
    cancellationToken);

        if (gatewayResult.CreationStatus == PaymentCreationStatus.Failed)
        {
            var failedResult = payment.MarkAsFailed();

            if (failedResult.IsFailure)
                return Result<CreatePaymentResponse>.Failure(failedResult.Error);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.ProviderFailed",
                    "iyzico rejected the checkout initialization."));
        }

        if (gatewayResult.CreationStatus == PaymentCreationStatus.Unknown)
        {
            if (!string.IsNullOrWhiteSpace(gatewayResult.CheckoutToken))
            {
                var unknownSessionResult = payment.SetCheckoutSession(
    gatewayResult.CheckoutToken,
    DateTime.UtcNow.AddMinutes(15));

                if (unknownSessionResult.IsFailure)
                {
                    return Result<CreatePaymentResponse>.Failure(
                        unknownSessionResult.Error);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.InitializationUnknown",
                    "Payment initialization could not be confirmed. Please try again later."));
        }

        if (string.IsNullOrWhiteSpace(gatewayResult.CheckoutToken) ||
            string.IsNullOrWhiteSpace(gatewayResult.PaymentUrl))
        {
            return Result<CreatePaymentResponse>.Failure(
                new Error(
                    "Payment.InvalidProviderResponse",
                    "Payment provider returned an invalid checkout session."));
        }

        var sessionResult = payment.SetCheckoutSession(
            gatewayResult.CheckoutToken,
            DateTime.UtcNow.AddMinutes(15));

        if (sessionResult.IsFailure)
        {
            return Result<CreatePaymentResponse>.Failure(
                sessionResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreatePaymentResponse>.Success(
            new CreatePaymentResponse(
                payment.Id.Value,
                order.Id.Value,
                payment.Amount.Amount,
                payment.Amount.Currency.ToString(),
                gatewayResult.PaymentUrl));
    }
}


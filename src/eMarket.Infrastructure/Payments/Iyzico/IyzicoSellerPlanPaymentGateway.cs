using System.Globalization;
using eMarket.Application.Common.Interfaces.Payments;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class IyzicoSellerPlanPaymentGateway
    : ISellerPlanPaymentGateway
{
    private readonly IyzicoOptions _options;

    public IyzicoSellerPlanPaymentGateway(
        IOptions<IyzicoOptions> options)
    {
        _options = options.Value;
    }

    public async Task<PaymentGatewayResult> CreatePaymentAsync(
        SellerPlanPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PaymentId == Guid.Empty ||
            request.Amount <= 0 ||
            string.IsNullOrWhiteSpace(request.ReturnUrl))
        {
            return new PaymentGatewayResult(
                false, null, null, "Invalid seller plan payment request.");
        }

        var options = CreateOptions();
        var buyer = new Buyer
        {
            Id = request.BuyerId,
            Name = request.BuyerName,
            Surname = request.BuyerSurname,
            Email = request.BuyerEmail,
            GsmNumber = NormalizeTurkishPhone(request.BuyerPhone),
            IdentityNumber = request.BuyerIdentityNumber,
            RegistrationAddress = request.BillingAddress.Description,
            City = request.BillingAddress.City,
            Country = request.BillingAddress.Country
        };

        var checkoutRequest = new CreateCheckoutFormInitializeRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = request.PaymentId.ToString(),
            BasketId = request.PaymentId.ToString(),
            Price = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            PaidPrice = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            Currency = request.Currency,
            PaymentGroup = PaymentGroup.LISTING.ToString(),
            CallbackUrl = request.CallbackUrl,
            Buyer = buyer,
            BillingAddress = new Iyzipay.Model.Address
            {
                ContactName = request.BillingAddress.ContactName,
                City = request.BillingAddress.City,
                Country = request.BillingAddress.Country,
                Description = request.BillingAddress.Description
            },
            BasketItems =
            [
                new BasketItem
                {
                    Id = request.PaymentId.ToString(),
                    Name = "Pazora Seller Plan",
                    Category1 = "Seller Subscription",
                    Category2 = "Pazora",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = request.Amount.ToString("0.00", CultureInfo.InvariantCulture)
                }
            ]
        };

        try
        {
            var response = await CheckoutFormInitialize.Create(
                checkoutRequest, options);

            if (!string.Equals(
                    response.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new PaymentGatewayResult(
                    false, null, response.Token,
                    response.ErrorMessage ?? "Checkout initialization failed.");
            }

            if (string.IsNullOrWhiteSpace(response.Token) ||
                string.IsNullOrWhiteSpace(response.PaymentPageUrl))
            {
                return new PaymentGatewayResult(
                    false, null, response.Token,
                    "iyzico did not return a checkout URL and token.");
            }

            return new PaymentGatewayResult(
                true, response.PaymentPageUrl, response.Token, null);
        }
        catch (Exception)
        {
            return new PaymentGatewayResult(
                false, null, null, "Unable to initialize seller plan payment.");
        }
    }
    public async Task<PaymentCheckoutResult> GetCheckoutResultAsync(
    string token,
    string conversationId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new PaymentCheckoutResult(
                false, null, "iyzico token is required.",
                null, null, null, null, null);
        }

        try
        {
            var response = await CheckoutForm.Retrieve(
    new RetrieveCheckoutFormRequest
    {
        Token = token,
        ConversationId = conversationId
    },
    CreateOptions());

            var success =
                string.Equals(response.Status, Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(response.PaymentStatus, "SUCCESS",
                    StringComparison.OrdinalIgnoreCase);

            return new PaymentCheckoutResult(
                success,
                response.PaymentId,
                success ? null : response.ErrorMessage ??
                    "Seller plan payment could not be verified.",
                response.ConversationId,
                response.BasketId,
                response.Price,
                response.PaidPrice,
                response.Currency);
        }
        catch (Exception)
        {
            return new PaymentCheckoutResult(
                false, null, "Unable to verify seller plan payment.",
                null, null, null, null, null);
        }
    }

    private Iyzipay.Options CreateOptions() => new()
    {
        ApiKey = _options.ApiKey,
        SecretKey = _options.SecretKey,
        BaseUrl = _options.BaseUrl
    };

    private static string NormalizeTurkishPhone(string phone)
    {
        var digits = new string(
            (phone ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digits.StartsWith("0090"))
            digits = digits[4..];
        else if (digits.StartsWith("90"))
            digits = digits[2..];
        else if (digits.StartsWith("0"))
            digits = digits[1..];

        if (digits.Length != 10)
            throw new ArgumentException("Invalid Turkish phone number.");

        return $"+90{digits}";
    }
}

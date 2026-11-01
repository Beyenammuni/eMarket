using eMarket.Application.Common.Interfaces.Payments;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Model.V2.Subscription;
using Iyzipay.Request.V2.Subscription;
using Microsoft.Extensions.Options;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class IyzicoSubscriptionGateway
    : ISubscriptionPaymentGateway
{
    private readonly IyzicoOptions _options;

    public IyzicoSubscriptionGateway(
        IOptions<IyzicoOptions> options)
    {
        _options = options.Value;
    }

    public async Task<SubscriptionPaymentGatewayResult>
        CreateCheckoutFormAsync(
            SubscriptionPaymentGatewayRequest request,
            CancellationToken cancellationToken = default)
    {
        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        try
        {
            var pricingPlanReferenceCode =
                await GetOrCreatePricingPlanAsync(
                    request.MonthlyAmount,
                    request.Currency,
                    options);

            if (string.IsNullOrWhiteSpace(pricingPlanReferenceCode))
            {
                return new SubscriptionPaymentGatewayResult(
                    false,
                    null,
                    null,
                    "Could not find or create an iyzico subscription pricing plan.");
            }

            var customer = new CheckoutFormCustomer
            {
                Name = request.Buyer.Name,
                Surname = request.Buyer.Surname,
                Email = request.Buyer.Email,
                GsmNumber = NormalizeTurkishPhone(request.Buyer.Phone),
                IdentityNumber = request.Buyer.IdentityNumber,
                BillingAddress = CreateAddress(request.Address),
                ShippingAddress = CreateAddress(request.Address)
            };

            var checkoutRequest =
                new InitializeCheckoutFormRequest
                {
                    ConversationId = request.PaymentId.ToString(),
                    CallbackUrl = request.ReturnUrl,
                    PricingPlanReferenceCode =
                        pricingPlanReferenceCode,
                    SubscriptionInitialStatus =
    SubscriptionStatus.ACTIVE.ToString(),
                    Customer = customer
                };

            var response =
                Subscription.InitializeCheckoutForm(
                    checkoutRequest,
                    options);


            if (!string.Equals(
                    response.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new SubscriptionPaymentGatewayResult(
                    false,
                    null,
                    null,
                    response.ErrorMessage ??
                    "iyzico subscription checkout initialization failed.");
            }

            return new SubscriptionPaymentGatewayResult(
     true,
     response.CheckoutFormContent,
     response.Token,
     null)
            {
                PricingPlanReferenceCode = pricingPlanReferenceCode
            };
        }
        catch (Exception ex)
        {
            return new SubscriptionPaymentGatewayResult(
                false,
                null,
                null,
                ex.Message);
        }
    }

    private async Task<string?> GetOrCreatePricingPlanAsync(
    decimal monthlyAmount,
    string currency,
    Iyzipay.Options options)
    {
        var productReferenceCode =
    _options.SubscriptionProductReferenceCode;


        if (string.IsNullOrWhiteSpace(productReferenceCode))
        {
            throw new InvalidOperationException(
                "iyzico SubscriptionProductReferenceCode is not configured.");
        }

        var plansRequest =
            new RetrieveAllPlanRequest
            {
                ConversationId = Guid.NewGuid().ToString(),
                ProductReferenceCode = productReferenceCode,
                Page = 1,
                Count = 100
            };

        var plansResponse =
            Plan.RetrieveAll(
                plansRequest,
                options);
        if (!string.Equals(
        plansResponse.Status,
        Status.SUCCESS.ToString(),
        StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"iyzico Plan.RetrieveAll failed. " +
                $"Status: {plansResponse.Status}, " +
                $"ErrorCode: {plansResponse.ErrorCode}, " +
                $"ErrorMessage: {plansResponse.ErrorMessage}");
        }

        var existingPlan =
     plansResponse.Data?.Items?
         .FirstOrDefault(x =>
             decimal.TryParse(
                 x.Price,
                 System.Globalization.NumberStyles.Any,
                 System.Globalization.CultureInfo.InvariantCulture,
                 out var price) &&
             Math.Abs(price - monthlyAmount) < 0.01m &&
             string.Equals(
                 x.CurrencyCode,
                 currency,
                 StringComparison.OrdinalIgnoreCase) &&
             string.Equals(
                 x.PaymentInterval,
                 "MONTHLY",
                 StringComparison.OrdinalIgnoreCase) &&
             x.PaymentIntervalCount == 1);

        if (existingPlan is not null)
            return existingPlan.ReferenceCode;

        var createPlanRequest =
            new CreatePlanRequest
            {
                ConversationId = Guid.NewGuid().ToString(),
                ProductReferenceCode = productReferenceCode,
                Name = $"eMarket Monthly {monthlyAmount:0.00} {currency}",
                Price = monthlyAmount.ToString(
                    "0.00",
                    System.Globalization.CultureInfo.InvariantCulture),
                CurrencyCode = currency,
                PaymentInterval = "MONTHLY",
                PaymentIntervalCount = 1,
                TrialPeriodDays = 0,
                PlanPaymentType = "RECURRING",
                RecurrenceCount = null
            };

        var createPlanResponse =
            Plan.Create(
                createPlanRequest,
                options);

        if (!string.Equals(
        createPlanResponse.Status,
        Status.SUCCESS.ToString(),
        StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"iyzico Plan.Create failed. " +
                $"Status: {createPlanResponse.Status}, " +
                $"ErrorCode: {createPlanResponse.ErrorCode}, " +
                $"ErrorMessage: {createPlanResponse.ErrorMessage}");
        }

        return createPlanResponse.Data?.ReferenceCode;
    }

    private static Address CreateAddress(
        SubscriptionPaymentAddress address)
    {
        return new Address
        {
            ContactName = address.FullName,
            City = address.City,
            Country = "Turkey",
            ZipCode = address.PostalCode,
            Description = BuildAddressDescription(address)
        };
    }

    private static string BuildAddressDescription(
        SubscriptionPaymentAddress address)
    {
        var parts = new[]
        {
            address.AddressLine,
            address.Neighborhood,
            address.Street,
            address.BuildingNumber,
            address.ApartmentNumber,
            address.District
        };

        return string.Join(
            ", ",
            parts.Where(x =>
                !string.IsNullOrWhiteSpace(x)));
    }

    public async Task<SubscriptionCheckoutResult>
    GetCheckoutResultAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };

        try
        {
            var request = new GetCheckoutFormResultRequest
            {
                Token = token
            };

            var response =
                Subscription.GetCheckoutFormResult(
                    request,
                    options);


            if (!string.Equals(
                    response.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return new SubscriptionCheckoutResult(
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    response.ErrorMessage ??
                    "iyzico subscription checkout result failed.");
            }

            var data = response.Data;

            if (data is null ||
                string.IsNullOrWhiteSpace(data.ReferenceCode) ||
                !Guid.TryParse(response.ConversationId, out var parsedPaymentId))
            {
                return new SubscriptionCheckoutResult(
                    false, null, null, null, null, null,
                    "Invalid subscription checkout result.");
            }

            // Retrieve the subscription from iyzico.
            var retrieveRequest = new RetrieveSubscriptionRequest
            {
                SubscriptionReferenceCode = data.ReferenceCode
            };

            var retrieveResponse = Subscription.Retrieve(
                retrieveRequest,
                options);

            if (!string.Equals(
                    retrieveResponse.Status,
                    Status.SUCCESS.ToString(),
                    StringComparison.OrdinalIgnoreCase) ||
                retrieveResponse.Data is null ||
                !string.Equals(
                    retrieveResponse.Data.ReferenceCode,
                    data.ReferenceCode,
                    StringComparison.Ordinal))
            {
                return new SubscriptionCheckoutResult(
                    false, null, null, null, null, null,
                    "Unable to verify the subscription with iyzico.");
            }

            // Require a successful payment attempt linked to this payment.
            var successfulAttempt = retrieveResponse.Data.SubscriptionOrders?
                .SelectMany(order => order.OrderPaymentAttempts ?? [])
                .Any(attempt =>
                    string.Equals(
                        attempt.ConversationId,
                        response.ConversationId,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        attempt.PaymentStatus,
                        "SUCCESS",
                        StringComparison.OrdinalIgnoreCase));

            if (successfulAttempt != true)
            {
                return new SubscriptionCheckoutResult(
                    false, null, null, null, null, null,
                    "The initial subscription payment could not be verified.");
            }

            return new SubscriptionCheckoutResult(
                true,
                parsedPaymentId,
                data.ReferenceCode,
                data.PricingPlanReferenceCode,
                data.CustomerReferenceCode,
                retrieveResponse.Data.SubscriptionStatus,
                null);
        }catch (Exception ex)
        {
            return new SubscriptionCheckoutResult(
                false, null, null, null, null, null,
                ex.Message);
        }
    }
    private static string NormalizeTurkishPhone(string phone)
    {
        var value = phone.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        if (value.StartsWith("+90"))
            return value;

        if (value.StartsWith("90"))
            return "+" + value;

        if (value.StartsWith("0"))
            return "+90" + value[1..];

        return "+90" + value;
    }
}

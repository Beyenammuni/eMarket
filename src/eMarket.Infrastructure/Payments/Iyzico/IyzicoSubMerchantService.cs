using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Businesses.ValueObjects;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

namespace eMarket.Infrastructure.Payments.Iyzico;

internal sealed class IyzicoSubMerchantService
    : IIyzicoSubMerchantService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IyzicoOptions _options;

    public IyzicoSubMerchantService(
        IBusinessRepository businessRepository,
        IOptions<IyzicoOptions> options)
    {
        _businessRepository = businessRepository;
        _options = options.Value;
    }

    public async Task<SubMerchantProvisioningResult> CreateAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var business = await _businessRepository.GetByIdAsync(
            BusinessId.Create(businessId),
            cancellationToken);

        if (business is null)
        {
            return new SubMerchantProvisioningResult(
                false,
                null,
                null,
                "Business was not found.");
        }

        if (!string.IsNullOrWhiteSpace(
                business.IyzicoSubMerchantKey))
        {
            return new SubMerchantProvisioningResult(
                true,
                business.IyzicoSubMerchantKey,
                business.IyzicoSubMerchantStatus,
                null);
        }

        var merchant = business.MerchantDetails;

        var options = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl
        };
        var request = new CreateSubMerchantRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = business.Id.Value.ToString(),

            SubMerchantExternalId =
                business.Id.Value.ToString(),

            SubMerchantType =
                MapMerchantType(merchant.MerchantType),

            Address = merchant.Address,
            ContactName = GetContactName(merchant.LegalName),
            ContactSurname = GetContactSurname(merchant.LegalName),
            Email = merchant.Email,
            GsmNumber = merchant.PhoneNumber,
            Name = merchant.LegalName,
            Iban = merchant.Iban,

            Currency = Currency.TRY.ToString()
        };

        if (merchant.MerchantType == MerchantType.Personal)
        {
            request.IdentityNumber =
                merchant.IdentityNumber;
        }
        else
        {
            request.TaxNumber =
                merchant.TaxNumber;

            request.TaxOffice =
                merchant.TaxOffice;

            request.LegalCompanyTitle =
                merchant.LegalName;
        }

        try
        {
           
            var response =
                await SubMerchant.Create(
                    request,
                    options);

            if (!string.Equals(
        response.Status,
        Status.SUCCESS.ToString(),
        StringComparison.OrdinalIgnoreCase))
            {
                return new SubMerchantProvisioningResult(
                    false,
                    null,
                    response.Status,
                    $"iyzico error. Code: {response.ErrorCode}, " +
                    $"Message: {response.ErrorMessage}");
            }
            return new SubMerchantProvisioningResult(
                true,
                response.SubMerchantKey,
                response.Status,
                null);
        }
        catch (Exception ex)
        {
            return new SubMerchantProvisioningResult(
                false,
                null,
                null,
                ex.Message);
        }
    }

    private static string MapMerchantType(
        MerchantType merchantType)
    {
        if (merchantType == MerchantType.Personal)
        {
            return "PERSONAL";
        }

        if (merchantType == MerchantType.PrivateCompany)
        {
            return "PRIVATE_COMPANY";
        }

        if (merchantType ==
            MerchantType.LimitedOrJointStockCompany)
        {
            return "LIMITED_OR_JOINT_STOCK_COMPANY";
        }

        throw new InvalidOperationException(
            $"Unsupported merchant type: {merchantType.Name}");
    }

    private static string GetContactName(
        string legalName)
    {
        var parts = legalName
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        return parts.Length > 1
            ? string.Join(
                ' ',
                parts.Take(parts.Length - 1))
            : legalName;
    }

    private static string GetContactSurname(
        string legalName)
    {
        var parts = legalName
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        return parts.Length > 1
            ? parts[^1]
            : legalName;
    }
}


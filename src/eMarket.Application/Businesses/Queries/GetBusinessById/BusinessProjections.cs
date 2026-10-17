using eMarket.Domain.Businesses;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Application.Businesses.Queries.GetBusinessById;

internal static class BusinessProjections
{

public static IQueryable<GetBusinessByIdResponse> ToGetBusinessByIdResponse(
    this IQueryable<Business> query)
    {
        return query.Select(x => new GetBusinessByIdResponse(
            x.Id.Value,
            x.Name.Value,
            x.Type.Id,
            x.Type.Name,
            x.Status.ToString(),
            x.SupportsSubscriptions,
            x.CreatedAt,
            x.MerchantDetails.Email,
            x.MerchantDetails.PhoneNumber,
            x.MerchantDetails.Address,
            x.MerchantDetails.City,
x.MerchantDetails.Country,
x.MerchantDetails.LegalName,
x.MerchantDetails.IdentityNumber,
x.MerchantDetails.TaxNumber,
x.MerchantDetails.TaxOffice));
    }
}

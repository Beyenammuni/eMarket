namespace eMarket.Api.Endpoinds.Categories.Models
{
    public record PagenationRequest
    (
        int Page = 1,
        int pageSize = 10
        );
}

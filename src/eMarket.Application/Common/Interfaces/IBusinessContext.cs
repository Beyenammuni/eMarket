using eMarket.Domain.Businesses;

namespace eMarket.Application.Catalog.Categories.Commands.UpdateCategory;

public interface IBusinessContext
{
    BusinessId? BusinessId { get; }

    bool HasBusiness { get; }
}

using FluentValidation;

namespace eMarket.Application.Businesses.Queries.GetBusinessById;

public sealed class GetBusinessByIdValidator
    : AbstractValidator<GetBusinessByIdQuery>
{
    public GetBusinessByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

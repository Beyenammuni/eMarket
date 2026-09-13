using FluentValidation;

namespace eMarket.Application.Sales.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Neighborhood)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.ApartmentNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .NotEqual(0);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .NotEqual(0);
    }
}

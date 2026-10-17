using eMarket.Application.Identity.Auth.Commands;
using FluentValidation;

namespace eMarket.Application.Identity.Addresses.Commands;

public sealed class CreateUserAddressCommandValidator
    : AbstractValidator<CreateUserAddressCommand>
{
    public CreateUserAddressCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20);

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
            .MaximumLength(150);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.ApartmentNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);
    }
}

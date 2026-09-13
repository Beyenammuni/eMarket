using FluentValidation;

namespace eMarket.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionValidator
    : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.FirstDeliveryDate)
            .Must(x => x.Date >= DateTime.UtcNow.Date)
            .WithMessage("First delivery date cannot be in the past.");

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

        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(x => x.Count <= 50)
            .WithMessage("A subscription cannot contain more than 50 items.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty();

                item.RuleFor(x => x.Quantity)
                    .InclusiveBetween(1, 1000);
            });
    }
}

using FluentValidation;
namespace eMarket.Application.Subscriptions.Commands.UpdateSubscription;
public sealed class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscriptionCommand>
{
    public UpdateSubscriptionValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().Must(x => x.Count <= 50);
        RuleForEach(x => x.Items).ChildRules(item => item.RuleFor(x => x.Quantity).InclusiveBetween(1, 1000));
    }
}

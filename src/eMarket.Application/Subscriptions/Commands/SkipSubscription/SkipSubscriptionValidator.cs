using FluentValidation;
namespace eMarket.Application.Subscriptions.Commands.SkipSubscription;
public sealed class SkipSubscriptionValidator : AbstractValidator<SkipSubscriptionCommand> { public SkipSubscriptionValidator(){ RuleFor(x=>x.SubscriptionId).NotEmpty(); } }

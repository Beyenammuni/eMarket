using FluentValidation;
namespace eMarket.Application.Subscriptions.Commands.CancelSubscription;
public sealed class CancelSubscriptionValidator : AbstractValidator<CancelSubscriptionCommand> { public CancelSubscriptionValidator(){ RuleFor(x=>x.SubscriptionId).NotEmpty(); } }

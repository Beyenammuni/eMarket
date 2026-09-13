using FluentValidation;
namespace eMarket.Application.Subscriptions.Commands.GenerateSubscriptionOrder;
public sealed class GenerateSubscriptionOrderValidator : AbstractValidator<GenerateSubscriptionOrderCommand> { public GenerateSubscriptionOrderValidator(){RuleFor(x=>x.SubscriptionId).NotEmpty();} }

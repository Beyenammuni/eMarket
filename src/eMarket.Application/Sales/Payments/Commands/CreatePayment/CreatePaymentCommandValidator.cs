using FluentValidation;

namespace eMarket.Application.Sales.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandValidator
    : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .MaximumLength(500);
    }
}

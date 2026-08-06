using FluentValidation;

namespace eMarket.Application.Businesses.Commands.TransferOwnership;

public sealed class TransferOwnershipValidator
    : AbstractValidator<TransferOwnershipCommand>
{
    public TransferOwnershipValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.NewOwnerId)
            .NotEmpty();
    }
}

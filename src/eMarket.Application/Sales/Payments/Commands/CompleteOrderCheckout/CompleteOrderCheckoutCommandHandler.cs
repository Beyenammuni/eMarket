using eMarket.Application.Common.IRepositories;
using eMarket.Application.Sales.Payments.Services;
using eMarket.Domain.Payments;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Payments.Commands.CompleteOrderCheckout;

internal sealed class CompleteOrderCheckoutCommandHandler
    : IRequestHandler<CompleteOrderCheckoutCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly OrderPaymentConfirmationService _confirmationService;

    public CompleteOrderCheckoutCommandHandler(
        IPaymentRepository paymentRepository,
        OrderPaymentConfirmationService confirmationService)
    {
        _paymentRepository = paymentRepository;
        _confirmationService = confirmationService;
    }

    public async Task<Result> Handle(
        CompleteOrderCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(
            PaymentId.Create(request.PaymentId),
            cancellationToken);

        if (payment is null)
            return Result.Failure(PaymentErrors.NotFound);

        return await _confirmationService.ConfirmAsync(
            payment,
            request.Token,
            cancellationToken);
    }
}

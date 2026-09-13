using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.StartProcessing;

public sealed record StartProcessingCommand(
    Guid Id) : IRequest<Result>;

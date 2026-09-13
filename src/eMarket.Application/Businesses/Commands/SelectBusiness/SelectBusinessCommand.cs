using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.SelectBusiness;

public sealed record SelectBusinessCommand(
    Guid BusinessId)
    : IRequest<Result<SelectBusinessResponse>>;

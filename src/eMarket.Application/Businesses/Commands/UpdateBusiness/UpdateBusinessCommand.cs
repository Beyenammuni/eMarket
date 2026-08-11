using eMarket.Application.Businesses.Commands.UpdateBusiness;
using eMarket.SharedKernel.Results;
using MediatR;

public sealed record UpdateBusinessCommand(
    Guid BusinessId,
    string Name,
    int Type)
    : IRequest<Result<UpdateBusinessResponse>>;

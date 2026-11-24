using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Commands.CreateBranch;

public sealed record CreateBranchCommand(
    Guid BusinessId,
    string Name,
    string Address,
    string City,
    string District,
    string PhoneNumber,
    decimal? Latitude,
    decimal? Longitude
) : IRequest<Result<Guid>>;

using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Commands.UpdateBranch;

public sealed record UpdateBranchCommand(
    Guid BusinessId,
    Guid BranchId,
    string Name,
    string Address,
    string City,
    string District,
    string PhoneNumber,
    decimal? Latitude,
    decimal? Longitude
) : IRequest<Result>;

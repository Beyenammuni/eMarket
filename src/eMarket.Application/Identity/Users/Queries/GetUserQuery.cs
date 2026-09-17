using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Users.Queries.GetUser;

public sealed record GetUserQuery(
    Guid UserId
) : IRequest<Result<GetUserResponse>>;

public sealed record GetUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Username
);

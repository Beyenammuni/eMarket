using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Users.GetUsers
{
    public sealed record GetUsersQuery
    : IRequest<Result<List<GetUsersResponse>>>;

    public sealed record GetUsersResponse(
        Guid Id,
        string FullName,
        string Email,
        string Username
    );
}

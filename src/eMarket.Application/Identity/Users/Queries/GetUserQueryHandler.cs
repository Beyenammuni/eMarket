using eMarket.Application.Common.IRepositories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Users.Queries.GetUser;

internal sealed class GetUserQueryHandler
    : IRequestHandler<GetUserQuery, Result<GetUserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<GetUserResponse>> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult =
            eMarket.Domain.Identity.UserId.Create(request.UserId);

        var user = await _userRepository.GetByIdAsync(
            userIdResult,
            cancellationToken);

        if (user is null)
        {
            return Result<GetUserResponse>.Failure(
                new Error(
                    "User.NotFound",
                    "User was not found."));
        }

        return Result<GetUserResponse>.Success(
            new GetUserResponse(
                user.Id.Value,
                user.FullName.Full,
                user.Email.Value,
                user.Username));
    }
}

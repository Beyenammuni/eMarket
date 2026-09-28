using eMarket.Application.Common.IRepositories;
using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Identity.Users.GetUsers
{
    internal sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, Result<List<GetUsersResponse>>>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<List<GetUsersResponse>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _userRepository.ListAsync(
                cancellationToken);

            var response = users
                .Select(user => new GetUsersResponse(
                    user.Id.Value,
                    user.FullName.Full,
                    user.Email.Value,
                    user.Username))
                .ToList();

            return Result<List<GetUsersResponse>>.Success(response);
        }
    }
}

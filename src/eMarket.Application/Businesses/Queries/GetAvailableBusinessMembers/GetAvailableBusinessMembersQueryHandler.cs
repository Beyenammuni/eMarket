using eMarket.Application.Common.IRepositories;
using eMarket.Domain.Businesses;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Application.Businesses.Queries.GetAvailableBusinessMembers
{
    internal sealed class GetAvailableBusinessMembersQueryHandler
     : IRequestHandler<
         GetAvailableBusinessMembersQuery,
         Result<List<GetAvailableBusinessMemberResponse>>>
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IUserRepository _userRepository;

        public GetAvailableBusinessMembersQueryHandler(
            IBusinessRepository businessRepository,
            IUserRepository userRepository)
        {
            _businessRepository = businessRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<List<GetAvailableBusinessMemberResponse>>> Handle(
            GetAvailableBusinessMembersQuery request,
            CancellationToken cancellationToken)
        {
            var businessIdResult =
                BusinessId.Create(request.BusinessId);

            var business = await _businessRepository.GetWithMembersAsync(
                businessIdResult,
                cancellationToken);

            if (business is null)
            {
                return Result<List<GetAvailableBusinessMemberResponse>>.Failure(
                    new Error(
                        "Business.NotFound",
                        "Business was not found."));
            }

            var users = await _userRepository.ListAsync(
                cancellationToken);

            var memberUserIds = business.Members
                .Select(member => member.UserId)
                .ToHashSet();

            var availableUsers = users
                .Where(user => !memberUserIds.Contains(user.Id))
                .Where(user =>
                    !user.Roles.Any(role =>
                        role.Id == UserRole.Admin.Id ||
                        role.Id == UserRole.SuperAdmin.Id))
                .Select(user =>
                    new GetAvailableBusinessMemberResponse(
                        user.Id.Value,
                        user.FullName.Full,
                        user.Email.Value,
                        user.Username))
                .ToList();

            return Result<List<GetAvailableBusinessMemberResponse>>.Success(
                availableUsers);
        }
    }
}

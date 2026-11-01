using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Identity.Users.SetCustomerIdentityNumber;

public sealed record SetCustomerIdentityNumberCommand(
    string IdentityNumber) : IRequest<Result>;

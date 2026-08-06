namespace eMarket.Application.Common.Interfaces;

using eMarket.Domain.Identity;

public interface ICurrentUser
{
    UserId UserId { get; }

    bool IsAuthenticated { get; }

    string? Email { get; }
}

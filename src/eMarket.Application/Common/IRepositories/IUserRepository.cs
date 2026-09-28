using eMarket.Domain.Identity;

namespace eMarket.Application.Common.IRepositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        UserId id,
        CancellationToken cancellationToken = default);

    Task<List<User>> ListAsync(
    CancellationToken cancellationToken = default);
}

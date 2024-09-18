using Dialogs.Domain.Entities;

namespace Dialogs.Domain.Interfaces;
public interface IUserRepository
{
    Task<User> GetUserByIdAsync(string userId);
    Task<List<User>> SearchUsersAsync(string firstName, string lastName);
    Task CreateUserAsync(User user);
}
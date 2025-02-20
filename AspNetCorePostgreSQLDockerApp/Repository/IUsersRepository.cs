using AspNetCorePostgreSQLDockerApp.Models;
using System.Threading.Tasks;
using System;

namespace AspNetCorePostgreSQLDockerApp.Repository
{
    public interface IUsersRepository
    {
        Task<User> GetUserByIdAsync(Guid userId);
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);

    }

}

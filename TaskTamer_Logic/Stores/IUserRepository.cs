using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores;

public interface IUserRepository
{
    
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
 
    Task<IEnumerable<User>> GetAllAsync();
    Task<int> AddAsync(User user);
    Task<int> UpdateAsync(User user);
    Task<int> DeleteAsync(int id);

   
    Task<User?> GetUserWithRolesAsync(string role);
    Task<IEnumerable<User>> GetUserWithRolesAllAsync(int id);
    Task UpdateLastLoginDateAsync(int userId, DateTime loginDate);
    Task UpdatePasswordHashAsync(int userId, string passwordHash, string salt);
    Task LockUserAsync(int userId, DateTime? lockEndDate);
    Task UnlockUserAsync(int userId);
    
}
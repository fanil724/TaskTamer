using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IAuthlogRepository
    {
        Task<AuthLog?> GetByIdAsync(int id);

        Task<IEnumerable<AuthLog>> GetAllAsync();
        Task<int> AddAsync(AuthLog auth);
        Task<int> UpdateAsync(AuthLog auth);
        Task<int> DeleteAsync(int id);
    }
}

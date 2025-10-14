using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IPositionRepository
    {
        Task<Position?> GetByIdAsync(int id);
        Task<IEnumerable<Position>> GetAllAsync();
        Task<int> AddAsync(Position position);
        Task<int> UpdateAsync(Position position);
        Task<int> DeleteAsync(int id);
        Task<Position?> GetByNameAsync(string name);
    }
}

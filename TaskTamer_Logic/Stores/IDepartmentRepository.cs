using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IDepartmentRepository
    {

        Task<Department?> GetByIdAsync(int id);
        Task<Department?> GetByNameAsync(string name);
        Task<IEnumerable<Department>> GetAllAsync();
        Task<int> AddAsync(Department department);
        Task<int> UpdateAsync(Department department);
        Task<int> DeleteAsync(int id);
    }
}

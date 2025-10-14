using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int id);

        Task<Employee?> GetByNameAsync(string name);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<int> AddAsync(Employee employee);
        Task<int> UpdateAsync(Employee employee);
        Task<int> DeleteAsync(int id);

        Task<IEnumerable<Employee>> GetEmployeeWithPositionAsync(int id);
    }
}

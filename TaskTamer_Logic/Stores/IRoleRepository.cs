using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores;

public interface IRoleRepository
{
    Task<Role> GetById(int id);
    Task<Role> GetByName(string name);
    Task<IEnumerable<Role>> GetAllAsync();

    Task<int> Create(Role role);
    Task<int> Update(Role role);
    Task<int> Delete(int id);
}
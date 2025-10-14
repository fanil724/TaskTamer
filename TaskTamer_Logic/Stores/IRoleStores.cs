using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores;

public interface IRoleStores
{
    Task<Role> GetById(int id);
    Task<Role> GetByName(string name);
}
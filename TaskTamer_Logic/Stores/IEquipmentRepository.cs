using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetByIdAsync(int id);
        Task<List<Equipment>> GetAllAsync();
        Task<int> AddAsync(Equipment equipment);
        Task<int> UpdateAsync(Equipment equipment);
        Task<int> DeleteAsync(int id);
    }
}

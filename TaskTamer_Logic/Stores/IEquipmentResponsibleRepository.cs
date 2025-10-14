using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    internal interface IEquipmentResponsibleRepository
    {
        Task<EquipmentResponsible> GetByIdAsync(int id);

        Task<IEnumerable<EquipmentResponsible>> GetAllAsync();
        Task<int> AddAsync(EquipmentResponsible equipmentResponsible);
        Task<int> UpdateAsync(EquipmentResponsible equipmentResponsible);
        Task<int> DeleteAsync(int id);
    }
}

using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext _context;

    public EquipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Equipment?> GetByIdAsync(int id)
    {
        return _context.Equipment
            .Include(e => e.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(e => e.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(e => e.Department).FirstOrDefaultAsync(x => x.EquipmentID == id);
    }

    public Task<List<Equipment>> GetAllAsync()
    {
        return _context.Equipment
            .Include(e => e.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(e => e.ResponsibleEmployee).ThenInclude(p => p.Department)
                .Include(e => e.Department).ToListAsync();
    }

    public async Task<int> AddAsync(Equipment equipment)
    {
        _context.Equipment.Add(equipment);
        await _context.SaveChangesAsync();
        return equipment.EquipmentID;
    }

    public Task<int> UpdateAsync(Equipment equipment)
    {
        _context.Equipment.Update(equipment);
        return _context.SaveChangesAsync();
    }

    public Task<int> DeleteAsync(int id)
    {
        _context.Equipment.Remove(_context.Equipment.Find(id));
        return _context.SaveChangesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _context;

    public DepartmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments.FirstOrDefaultAsync(x => x.DepartmentID == id);
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments.ToListAsync();
    }

    public Task<int> AddAsync(Department department)
    {
        _context.Departments.AddAsync(department);
        return _context.SaveChangesAsync();
    }

    public Task<int> UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        return _context.SaveChangesAsync();
    }

    public Task<int> DeleteAsync(int id)
    {
        _context.Departments.Remove(_context.Departments.Find(id));
        return _context.SaveChangesAsync();
    }

    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _context.Departments.FirstOrDefaultAsync(x => x.Name == name);
    }
}
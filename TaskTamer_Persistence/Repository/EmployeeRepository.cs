using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee.EmployeeID;
        }

        public async Task<int> DeleteAsync(int id)
        {
            _context.Employees.Remove(_context.Employees.Find(id));
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.Include(d => d.Department).Include(p => p.Position).ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees.Include(d => d.Department).Include(p => p.Position).FirstOrDefaultAsync(x => x.EmployeeID == id);
        }

        public async Task<Employee?> GetByNameAsync(string name)
        {
            return await _context.Employees.FirstOrDefaultAsync(x=>x.FullName==name);
        }

        public async Task<IEnumerable<Employee>> GetEmployeeWithPositionAsync(int id)
        {
            return await _context.Employees.Include(p => p.Position).Where(x => x.Position.PositionID == id).ToListAsync();
        }

        public async Task<int> UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            return await _context.SaveChangesAsync();
        }
    }
}

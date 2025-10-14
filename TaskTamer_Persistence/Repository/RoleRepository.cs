using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Create(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role.RoleID;
        }

        public async Task<int> Delete(int id)
        {
            _context.Roles.Remove(_context.Roles.Find(id));
           return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role?> GetById(int id)
        {
           return await _context.Roles.FirstOrDefaultAsync(x => x.RoleID == id);
        }

        public async Task<Role?> GetByName(string name)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<int> Update(Role role)
        {
            _context.Roles.Update(role);
            return await _context.SaveChangesAsync();
        }
    }
}

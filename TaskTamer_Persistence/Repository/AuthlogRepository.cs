using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;


namespace TaskTamer_Persistence.Repository
{
    public class AuthlogRepository : IAuthlogRepository
    {
        private readonly AppDbContext _context;

        public AuthlogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(AuthLog auth)
        {
            _context.AuthLogs.Add(auth);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            _context.AuthLogs.Remove(_context.AuthLogs.Find(id));
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuthLog>> GetAllAsync()
        {
            return await _context.AuthLogs.ToListAsync();
        }

        public Task<AuthLog?> GetByIdAsync(int id)
        {
            return _context.AuthLogs
           .Include(x => x.User).ThenInclude(x => x.Employee).ThenInclude(d => d.Department)
           .Include(x => x.User).ThenInclude(x => x.Employee).ThenInclude(p => p.Position)
           .Include(x => x.User).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.LogID == id);
        }

        public async Task<int> UpdateAsync(AuthLog auth)
        {
            _context.AuthLogs.Update(auth);
            return await _context.SaveChangesAsync();
        }
    }
}

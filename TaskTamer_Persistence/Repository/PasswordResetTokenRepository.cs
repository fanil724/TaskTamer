
using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<int> AddAsync(PasswordResetToken prt)
        {
            _context.PasswordResetTokens.Add(prt);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            _context.PasswordResetTokens.Remove(_context.PasswordResetTokens.Find(id));
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PasswordResetToken>> GetAllAsync()
        {
            return await _context.PasswordResetTokens.ToListAsync();
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
           .Include(x => x.User).ThenInclude(x => x.Employee).ThenInclude(d => d.Department)
           .Include(x => x.User).ThenInclude(x => x.Employee).ThenInclude(p => p.Position)
           .Include(x => x.User).ThenInclude(x => x.Role)
           .FirstOrDefaultAsync(t => t.Token == token &&
                                          !t.IsUsed &&
                                          t.ExpirationDate > DateTime.Now);
        }

        public async Task<int> UpdateAsync(PasswordResetToken prt)
        {
            _context.PasswordResetTokens.Update(prt);
            return await _context.SaveChangesAsync();
        }
    }
}

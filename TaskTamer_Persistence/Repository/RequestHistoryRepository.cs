using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class RequestHistoryRepository : IRequestHistoryRepository
    {
        private readonly AppDbContext _context;

        public RequestHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(RequestHistory requestHistory)
        {
            _context.RequestHistory.Add(requestHistory);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            _context.RequestHistory.Remove(_context.RequestHistory.Find(id));
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RequestHistory>> GetAllAsync()
        {
            return await _context.RequestHistory.ToListAsync();
        }

        public async Task<IEnumerable<RequestHistory?>> GetByIdAsync(int id)
        {
            return await _context.RequestHistory.Where(x => x.HistoryID == id).ToListAsync();
        }

        public async Task<int> UpdateAsync(RequestHistory requestHistory)
        {
            _context.RequestHistory.Update(requestHistory);
            return await _context.SaveChangesAsync();
        }
    }
}

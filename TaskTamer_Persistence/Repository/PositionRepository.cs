using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class PositionRepository : IPositionRepository
    {
        private readonly AppDbContext _context;

        public PositionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            return position.PositionID;
        }

        public async Task<int> DeleteAsync(int id)
        {
            _context.Positions.Remove(_context.Positions.Find(id));
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            return await _context.Positions.ToArrayAsync();
        }

        public Task<Position> GetByIdAsync(int id)
        {
            return _context.Positions.FirstOrDefaultAsync(x => x.PositionID == id);
        }

        public async Task<int> UpdateAsync(Position position)
        {
            _context.Positions.Update(position);
            return await _context.SaveChangesAsync();
        }

        public async Task<Position?> GetByNameAsync(string name)
        {
            return await _context.Positions.FirstOrDefaultAsync(x => x.Title == name);
        }

        

    }
}

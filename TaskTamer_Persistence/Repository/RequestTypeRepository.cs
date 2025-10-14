using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class RequestTypeRepository : IRequestTypeRepository
    {
        private readonly AppDbContext _context;

        public RequestTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RequestType?> GetByNameAsync(string name)
        {
            return await _context.RequestTypes.FirstOrDefaultAsync(x => x.Name == name);
        }

        async Task<int> IRequestTypeRepository.AddAsync(RequestType requestType)
        {
            _context.RequestTypes.Add(requestType);
            await _context.SaveChangesAsync();
            return requestType.RequestTypeID;
        }

        async Task<int> IRequestTypeRepository.DeleteAsync(int id)
        {
            _context.RequestTypes.Remove(_context.RequestTypes.Find(id));
            return await _context.SaveChangesAsync();
        }

        async Task<IEnumerable<RequestType>> IRequestTypeRepository.GetAllAsync()
        {
            return await _context.RequestTypes.ToListAsync();
        }

        async Task<RequestType?> IRequestTypeRepository.GetByIdAsync(int id)
        {
            return await _context.RequestTypes.FirstOrDefaultAsync(x=>x.RequestTypeID==id);
        }

        async Task<int> IRequestTypeRepository.UpdateAsync(RequestType requestType)
        {
            _context.RequestTypes.Update(requestType);
            return await _context.SaveChangesAsync();
        }
    }
}

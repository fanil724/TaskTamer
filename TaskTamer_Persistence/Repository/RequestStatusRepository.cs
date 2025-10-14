using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository
{
    public class RequestStatusRepository : IRequestStatusRepository
    {

        private readonly AppDbContext _context;

        public RequestStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RequestStatus?> GetByNameAsync(string name)
        {
            return await _context.RequestStatuses.FirstOrDefaultAsync(x => x.Name == name);
        }

        async Task<int> IRequestStatusRepository.AddAsync(RequestStatus requestStatus)
        {
            _context.RequestStatuses.Add(requestStatus);
            await _context.SaveChangesAsync();
            return requestStatus.RequestStatusID;

        }

        Task<int> IRequestStatusRepository.DeleteAsync(int id)
        {
            _context.RequestStatuses.Remove(_context.RequestStatuses.Find(id));
            return _context.SaveChangesAsync();
        }

        async Task<IEnumerable<RequestStatus>> IRequestStatusRepository.GetAllAsync()
        {
            return await _context.RequestStatuses.ToListAsync();
        }

        async Task<RequestStatus?> IRequestStatusRepository.GetByIdAsync(int id)
        {
            return await _context.RequestStatuses.FirstOrDefaultAsync(x => x.RequestStatusID == id);
        }

        async Task<int> IRequestStatusRepository.UpdateAsync(RequestStatus requestStatus)
        {
            _context.RequestStatuses.Update(requestStatus);
            return await _context.SaveChangesAsync();

        }
    }
}

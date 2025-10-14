using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository;

public class RequestRepository : IRequestRepository
{
    private readonly AppDbContext _context;

    public RequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Request?> GetByIdAsync(int id)
    {
        return _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .FirstOrDefaultAsync(x => x.RequestID == id);
    }

    public async Task<IEnumerable<Request>> GetAllAsync()
    {
        return await _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .ToListAsync();
    }

    public async Task<int> AddAsync(Request request)
    {
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();
        return request.RequestID;
    }

    public Task<int> UpdateAsync(Request request)
    {
        _context.Requests.Update(request);
        return _context.SaveChangesAsync();
    }

    public Task<int> DeleteAsync(int id)
    {
        _context.Requests.Remove(_context.Requests.Find(id));
        return _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Request>> GetByStatusWithDetailsAsync(int statusId)
    {
        return await _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .Where(x => x.RequestStatusID == statusId).ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetByAuthorWithDetailsAsync(int authorId)
    {
        return await _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .Where(x => x.AuthorID == authorId).ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetByExecutorWithDetailsAsync(int executorId)
    {
        return await _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .Where(x => x.ExecutorID == executorId).ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetRequestsByEquipmentAsync(int id)
    {
        return await _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .Where(x => x.EquipmentID == id).ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetRequestWithRequestTypeAsync(int id)
    {
        return await _context.Requests
            .Include(r => r.Author).ThenInclude(c => c.Position)
            .Include(r => r.Author).ThenInclude(c => c.Department)
            .Include(x => x.RequestType).Include(c => c.RequestStatus)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Position)
            .Include(v => v.Equipment).ThenInclude(u => u.ResponsibleEmployee).ThenInclude(p => p.Department)
            .Include(v => v.Equipment).ThenInclude(u => u.Department)
            .Include(r => r.Executor).ThenInclude(c => c.Position)
            .Include(r => r.Executor).ThenInclude(c => c.Department)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Position)
            .Include(h => h.History).ThenInclude(c => c.ChangedBy).ThenInclude(p => p.Department)
            .Include(h => h.History).ThenInclude(c => c.Status)
            .Where(x => x.RequestTypeID == id).ToListAsync();
    }
}
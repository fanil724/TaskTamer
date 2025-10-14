using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores;

public interface IRequestRepository
{
    
    Task<Request?> GetByIdAsync(int id);
   
    Task<IEnumerable<Request>> GetAllAsync();
    Task<int> AddAsync(Request request);
    Task<int> UpdateAsync(Request request);
    Task<int> DeleteAsync(int id);
    Task<IEnumerable<Request>> GetByStatusWithDetailsAsync(int statusId);
    Task<IEnumerable<Request>> GetByAuthorWithDetailsAsync(int authorId);
    Task<IEnumerable<Request>> GetByExecutorWithDetailsAsync(int executorId);
    Task<IEnumerable<Request>> GetRequestsByEquipmentAsync(int id);
    Task<IEnumerable<Request>> GetRequestWithRequestTypeAsync(int id);
}
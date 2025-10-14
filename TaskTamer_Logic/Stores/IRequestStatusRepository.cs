using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IRequestStatusRepository
    {
        Task<RequestStatus?> GetByIdAsync(int id);

        Task<IEnumerable<RequestStatus>> GetAllAsync();
        Task<int> AddAsync(RequestStatus requestStatus);
        Task<int> UpdateAsync(RequestStatus requestStatus);
        Task<int> DeleteAsync(int id);
        Task<RequestStatus?> GetByNameAsync(string name);
    }
}

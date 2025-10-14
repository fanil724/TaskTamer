using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IRequestTypeRepository
    {
        Task<RequestType?> GetByIdAsync(int id);

        Task<IEnumerable<RequestType>> GetAllAsync();
        Task<int> AddAsync(RequestType requestType);
        Task<int> UpdateAsync(RequestType requestType);
        Task<int> DeleteAsync(int id);
        Task<RequestType?> GetByNameAsync(string name);
    }
}

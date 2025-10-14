using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IRequestHistoryRepository
    {

        public Task<IEnumerable<RequestHistory?>> GetByIdAsync(int id);
        public Task<IEnumerable<RequestHistory>> GetAllAsync();
        public Task<int> AddAsync(RequestHistory requestHistory);
        public Task<int> UpdateAsync(RequestHistory requestHistory);
        public Task<int> DeleteAsync(int id);
    }
}

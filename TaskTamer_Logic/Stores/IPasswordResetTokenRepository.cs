using TaskTamer_Logic.Models;

namespace TaskTamer_Logic.Stores
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task<IEnumerable<PasswordResetToken>> GetAllAsync();
        Task<int> AddAsync(PasswordResetToken prt);
        Task<int> UpdateAsync(PasswordResetToken prt);
        Task<int> DeleteAsync(int id);
    }
}

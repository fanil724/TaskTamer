using Microsoft.EntityFrameworkCore;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;

namespace TaskTamer_Persistence.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _context.Users
            .Include(x => x.Employee).ThenInclude(d => d.Department)
            .Include(x => x.Employee).ThenInclude(p => p.Position)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserID == id);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _context.Users
            .Include(x => x.Employee).ThenInclude(d => d.Department)
            .Include(x => x.Employee).ThenInclude(p => p.Position)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(x => x.Employee).ThenInclude(d => d.Department)
            .Include(x => x.Employee).ThenInclude(p => p.Position)
            .Include(x => x.Role).ToListAsync();
    }

    public async Task<int> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.UserID;
    }

    public Task<int> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return _context.SaveChangesAsync();
    }

    public Task<int> DeleteAsync(int id)
    {
        _context.Users.Remove(_context.Users.Find(id));
        return _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserWithRolesAsync(string role)
    {
        return await _context.Users.Include(x => x.Role)
            .Include(x => x.Employee).ThenInclude(d => d.Department)
            .Include(x => x.Employee).ThenInclude(p => p.Position)
            .FirstOrDefaultAsync(x => x.Role.Name == role);
    }

    public async Task<IEnumerable<User>> GetUserWithRolesAllAsync(int id)
    {
        return await _context.Users.Include(x => x.Role).Where(x => x.Role.RoleID == id).ToListAsync();
    }

    public Task UpdateLastLoginDateAsync(int userId, DateTime loginDate)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePasswordHashAsync(int userId, string passwordHash, string salt)
    {
        throw new NotImplementedException();
    }

    public Task LockUserAsync(int userId, DateTime? lockEndDate)
    {
        throw new NotImplementedException();
    }

    public Task UnlockUserAsync(int userId)
    {
        throw new NotImplementedException();
    }
}
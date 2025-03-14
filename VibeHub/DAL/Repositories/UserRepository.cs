using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetList()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetById(Guid id)
    {
        return await _context.Users
            .Include(user => user.Room)
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task Add(User user)
    {
        _context.Users.Add(user);
        
        await _context.SaveChangesAsync();
    }

    public async Task Update(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user != null)
        {
            _context.Users.Remove(user);
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _context.Users.AnyAsync(e => e.Id == id);
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByNickname(string nickname)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Nickname == nickname);
    }
}
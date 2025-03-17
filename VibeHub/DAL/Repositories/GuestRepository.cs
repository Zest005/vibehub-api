using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Guest> _dbSet;

    public GuestRepository(AppDbContext context, ILogger<MusicRepository> logger)
    {
        _context = context;
        _dbSet = _context.Set<Guest>();
    }

    public async Task<Guest?> GetById(Guid id)
    {
        return await _dbSet.Include(guest => guest.Room)
            .FirstOrDefaultAsync(guest => guest.Id == id);
    }

    public async Task<Guest> Create(Guest guest)
    {
        await _dbSet.AddAsync(guest);
        await _context.SaveChangesAsync();
        
        return guest;
    }

    public async Task Update(Guest guest)
    {
        _context.Update(guest);

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guest guest)
    {
        _dbSet.Remove(guest);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _dbSet.AnyAsync(guest => guest.Id == id);
    }
}

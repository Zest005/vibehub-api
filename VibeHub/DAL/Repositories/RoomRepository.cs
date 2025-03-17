using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Room>> GetList()
    {
        return await _context.Rooms.ToListAsync();
    }

    public async Task<Room?> GetById(Guid id)
    {
        return await _context.Rooms
            .Include(room => room.Playlist)
            .Include(room => room.Settings)
            .FirstOrDefaultAsync(room => room.Id == id);
    }

    public async Task<Room?> GetByCode(string code)
    {
        return await _context.Rooms
            .Include(room => room.Playlist)
            .Include(room => room.Settings)
            .FirstOrDefaultAsync(room => room.Code == code);
    }

    public async Task Add(Room room)
    {
        _context.Rooms.Add(room);

        await _context.SaveChangesAsync();
    }

    public async Task Update(Room room)
    {
        _context.Entry(room).State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room != null)
        {
            _context.Rooms.Remove(room);

            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _context.Rooms.AnyAsync(e => e.Id == id);
    }
}
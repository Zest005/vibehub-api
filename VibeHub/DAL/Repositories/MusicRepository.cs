using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories;

public class MusicRepository : IMusicRepository
{
    private readonly ILogger<MusicRepository> _logger;
    private readonly AppDbContext _context;
    private readonly DbSet<Music> _dbSet;

    public MusicRepository(AppDbContext context, ILogger<MusicRepository> logger)
    {
        _context = context;
        _logger = logger;
        _dbSet = _context.Set<Music>();
    }

    public async Task<Music> GetById(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(music => music.Id == id);
    }

    public async Task<bool> AddRange(IEnumerable<Music> music)
    {
        await _dbSet.AddRangeAsync(music);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task DeleteRange(List<Music> music)
    {
        _dbSet.AttachRange(music);
        _dbSet.RemoveRange(music);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _dbSet.AnyAsync(music => music.Id == id);
    }
}
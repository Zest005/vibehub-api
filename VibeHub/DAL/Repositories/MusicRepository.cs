using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories;

public class MusicRepository : IMusicRepository
{
    private AppDbContext _context;
    private DbSet<Music> _dbSet;
    private readonly ILogger<MusicRepository> _logger;
    
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

    public async Task<IEnumerable<Music>> GetList()
    {
        return await _dbSet.Take(10).ToListAsync();
    }

    public async Task<Music> Add(Music music)
    {
        _dbSet.Add(music);
        await _context.SaveChangesAsync();
        return music;
    }

    public async Task<Music> Update(Music music)
    {
        _context.Entry(music).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        
        return music;
    }

    public async Task<bool> Delete(Music music)
    {
        _dbSet.Remove(music);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> Exists(Guid id)
    {
        return await _dbSet.AnyAsync(music => music.Id == id);
    }
}
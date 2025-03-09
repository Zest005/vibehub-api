using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories;

public class MessageHistoryRepository : IMessageHistoryRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<MessageHistory> _dbSet;
    private readonly ILogger<MessageHistory> _logger;
    
    public MessageHistoryRepository(AppDbContext context, ILogger<MessageHistory> logger)
    {
        _context = context;
        _logger = logger;
        _dbSet = _context.Set<MessageHistory>();
    }
    public async Task<bool> Add(MessageHistory messageHistory)
    {
        _dbSet.Add(messageHistory);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<MessageHistory>> GetList(Guid roomId)
    {
        return await _dbSet.Where(messageHistory => messageHistory.RoomId == roomId).ToListAsync();
    }
}
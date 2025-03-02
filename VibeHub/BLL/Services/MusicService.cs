using BLL.Abstractions.Interfaces;
using DAL.Abstractions.Interfaces; 
using Core.Models;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class MusicService : IMusicService
{
    private readonly IMusicRepository _repository;
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<MusicService> _logger;
    
    public MusicService(IFilterUtility filterUtility, ILogger<MusicService> logger, IMusicRepository repository)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _repository = repository;
    }
    public async Task<Music> GetById(Guid id)
    {
        return await _repository.GetById(id);
    }

    public async Task<IEnumerable<Music>> GetList()
    {
        return await _repository.GetList();
    }

    public async Task<Music> Add(Music music)
    {
        Guid guid = Guid.NewGuid();
        
        music.Id = guid;
        music = await _filterUtility.Filter(music);
        
        return await _repository.Add(music);
    }

    public async Task<Music> Update(Guid id, Music music)
    {
        var targetMusic = await _repository.GetById(id);
        if (targetMusic == null)
        {
            _logger.LogError($"Music with id {id} not found");
            return null;
        }
        
        music = await _filterUtility.Filter(music);
        
        targetMusic.Title = music.Title;
        targetMusic.Artist = music.Artist;
        targetMusic.Filename = music.Filename;
        
        return await _repository.Update(music);
    }

    public async Task<bool> Delete(Guid id)
    {
        var music = await _repository.GetById(id);
        if (music == null)
        {
            _logger.LogError($"Music with id {id} not found");
            return false;
        }
        
        return await _repository.Delete(music);
    }
}
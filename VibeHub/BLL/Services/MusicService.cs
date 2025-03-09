using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.Models;
using DAL.Abstractions.Interfaces;
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
        _logger.LogInformation("Getting music list");
        try
        {
            return await _repository.GetList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting music list: {ex.Message}");
            throw;
        }
    }

    public async Task<Music> Add(Music music)
    {
        _logger.LogInformation("Adding new music");
        try
        {
            var guid = Guid.NewGuid();

            music.Id = guid;
            music = await _filterUtility.Filter(music);
            music = await _repository.Add(music);
            _logger.LogInformation($"Music with id {guid} added");
            return music;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding new music: {ex.Message}");
            throw;
        }
    }

    public async Task<Music> Update(Guid id, Music music)
    {
        try
        {
            _logger.LogInformation($"Updating music with id {id}");
            var targetMusic = await _repository.GetById(id);
            if (targetMusic == null)
            {
                _logger.LogError($"Music with id {id} not found");
                throw new InvalidOperationException($"Music with id {id} not found");
            }

            music = await _filterUtility.Filter(music);

            targetMusic.Title = music.Title;
            targetMusic.Artist = music.Artist;
            targetMusic.Filename = music.Filename;

            return await _repository.Update(targetMusic);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating music with id {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        _logger.LogInformation($"Deleting music with id {id}");
        try
        {
            var music = await _repository.GetById(id);
            if (music == null)
            {
                _logger.LogError($"Music with id {id} not found");
                return false;
            }

            return await _repository.Delete(music);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting music with id {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> Exists(Guid id)
    {
        _logger.LogInformation($"Checking if music with id {id} exists");
        try
        {
            return await _repository.Exists(id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking music existence with id {id}: {ex.Message}");
            throw;
        }
    }
}
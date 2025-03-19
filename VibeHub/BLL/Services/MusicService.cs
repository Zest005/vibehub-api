using BLL.Abstractions.Helpers;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.Errors;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace BLL.Services;

public class MusicService : IMusicService
{
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<MusicService> _logger;
    private readonly IMusicFileHelper _musicFileHelper;
    private readonly IMusicRepository _repository;

    public MusicService(IFilterUtility filterUtility, ILogger<MusicService> logger, IMusicRepository repository,
        IMusicFileHelper musicFileHelper)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _repository = repository;
        _musicFileHelper = musicFileHelper;
    }

    public async Task<EntityResult<Music>> GetById(Guid id)
    {
        try
        {
            var result = new EntityResult<Music>
            {
                Entity = await _repository.GetById(id)
            };

            if (result.Entity != null)
                return result;

            _logger.LogError($"Music with id {id} not found");

            return ErrorCatalog.MusicNotFound;
        }
        catch
        {
            _logger.LogError("An error occurred while getting the music by id.");
            return new EntityResult<Music>("Unknown error occurred", true);
        }
    }


    public async Task<EntityResult<FileStreamResult>> GetFileById(Guid id)
    {
        try
        {
            var targetMusic = await _repository.GetById(id);
            if (targetMusic == null)
            {
                _logger.LogError($"Music with id {id} not found");
                return ErrorCatalog.MusicFileNotFound;
            }

            var targetFileName = targetMusic.Id + ".*";

            return new EntityResult<FileStreamResult>
            {
                Entity = await _musicFileHelper.TryGetFile(targetFileName)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while getting the music file by id: {ex.Message}");
            return new EntityResult<FileStreamResult>("Unknown error occurred", true);
        }
    }


    public async Task<IEnumerable<Music>> AddRange(List<IFormFile> musicList, Guid roomId)
    {
        List<Music> playList = [];

        if (!musicList.All(music => _musicFileHelper.IsMusicFile(music).GetAwaiter().GetResult()))
        {
            _logger.LogError("The music files must be only.");
            throw new ArgumentException("The music files must be only.");
        }

        try
        {
            foreach (var musicFile in musicList)
            {
                var music = await _musicFileHelper.AddMusicFile(musicFile);
                music.RoomId = roomId;
                playList.Add(music);
            }

            playList = await _filterUtility.FilterCollection(playList);
            await _repository.AddRange(playList);

            return playList;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding music: {ex.Message}");
            return null;
        }
    }

    public async Task<Music> Delete(Music music)
    {
        try
        {
            await _repository.Delete(music);
            return music;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting music: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteRange(List<Music> musicList)
    {
        try
        {
            await _repository.DeleteRange(musicList);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting music: {ex.Message}");
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
using BLL.Abstractions.Helpers;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
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

    public async Task<Music> GetById(Guid id)
    {
        return await _repository.GetById(id);
    }


    public async Task<FileStreamResult> GetFileById(Guid id)
    {
        var targetMusic = await _repository.GetById(id);
        if (targetMusic == null)
        {
            _logger.LogError($"Music with id {id} not found");
            throw new InvalidOperationException($"Music with id {id} not found");
        }

        var targetFileName = targetMusic.Id + ".*";

        return await _musicFileHelper.TryGetFile(targetFileName);
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
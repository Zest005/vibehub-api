using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TagLib;
using File = System.IO.File;

namespace BLL.Services;

public class MusicService : IMusicService
{
    private readonly IMusicRepository _repository;
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<MusicService> _logger;
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");


    private readonly Dictionary<string, string> _allowedMimeTypes = new()
    {
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".flac", "audio/flac" }
    };

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


    public async Task<FileStreamResult> GetFileById(Guid id)
    {
        var targetMusic = await _repository.GetById(id);
        if (targetMusic == null)
        {
            _logger.LogError($"Music with id {id} not found");
            throw new InvalidOperationException($"Music with id {id} not found");
        }

        var filePath = Directory.GetFiles(_uploadPath, targetMusic.Id.ToString() + ".*");

        if (!File.Exists(filePath[0]))
        {
            _logger.LogError($"File {targetMusic.Id.ToString() + ".*"} not found at {filePath}");
            throw new InvalidOperationException($"File {targetMusic.Id.ToString() + ".*"} not found.");
        }

        var fileExtension = Path.GetExtension(filePath[0]).ToLower();
        var mimeType = _allowedMimeTypes.GetValueOrDefault(fileExtension, "application/octet-stream");  

        var fileStream = new FileStream(filePath[0], FileMode.Open, FileAccess.Read, FileShare.Read);

        return new FileStreamResult(fileStream, mimeType)
        {
            FileDownloadName = targetMusic.Id.ToString() + fileExtension 
        };
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

    public async Task<Music> Add(MusicDto musicDto)
    {
        _logger.LogInformation("Adding new music");
        try
        {
            var fileExtension = Path.GetExtension(musicDto.File.FileName).ToLower();


            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);

            if (!_allowedMimeTypes.ContainsKey(fileExtension) &&
                musicDto.File.ContentType != _allowedMimeTypes[fileExtension])
            {
                _logger.LogError("Invalid file or MIME type.");
                throw new Exception("Invalid file MIME type.");
            }

            var songTitle = Path.GetFileNameWithoutExtension(musicDto.File.FileName);
            var artistName = "Unknown Artist";

            var guid = Guid.NewGuid();
            var secretFileName = $"{guid}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, secretFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
               await musicDto.File.CopyToAsync(stream);
            }
            
            try
            {
                using var tagFile = TagLib.File.Create(filePath, ReadStyle.Average);
            
                if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title))
                {
                    songTitle = tagFile.Tag.Title;
                }
                if (tagFile.Tag.Performers.Length > 0)
                {
                    artistName = tagFile.Tag.Performers[0];
                }
                
            }
            finally { }

            var music = new Music
            {
                Id = guid,
                Title = songTitle,
                Artist = artistName
            };

            music = await _filterUtility.Filter(music);
            music = await _repository.Add(music);
            _logger.LogInformation($"Music with id {guid} added");

            return music;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding new music: {ex.Message}");
            throw new Exception($"Error adding new music: {ex.Message}");
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
            
            var filePath = Directory.GetFiles(_uploadPath, music.Id.ToString() + ".*");
            
            if (File.Exists(filePath[0]))
                File.Delete(filePath[0]);
            
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

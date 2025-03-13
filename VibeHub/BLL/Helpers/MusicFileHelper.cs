using BLL.Abstractions.Helpers;
using BLL.Abstractions.Utilities;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TagLib;
using File = System.IO.File;

namespace BLL.Helpers;

internal class MusicFileHelper : IMusicFileHelper
{
    private readonly Dictionary<string, string> _allowedMimeTypes = new()
    {
        { ".mp3", "audio/mpeg" },
        { ".wav", "audio/wav" },
        { ".flac", "audio/flac" }
    };


    private readonly ILogger<MusicFileHelper> _logger;
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

    public MusicFileHelper(ILogger<MusicFileHelper> logger)
    {
        _logger = logger;
    }

    public async Task<Music> AddMusicFile(IFormFile musicFile)
    {
        var fileExtension = Path.GetExtension(musicFile.FileName).ToLower();

        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);


        var songTitle = Path.GetFileNameWithoutExtension(musicFile.FileName);
        var artistName = "Unknown Artist";

        var guid = Guid.NewGuid();
        var secretFileName = $"{guid}{fileExtension}";
        var filePath = Path.Combine(_uploadPath, secretFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await musicFile.CopyToAsync(stream);
        }

        using var tagFile = TagLib.File.Create(filePath, ReadStyle.Average);

        if (!string.IsNullOrWhiteSpace(tagFile.Tag.Title)) songTitle = tagFile.Tag.Title;
        if (tagFile.Tag.Performers.Length > 0) artistName = tagFile.Tag.Performers[0];

        var music = new Music
        {
            Id = guid,
            Title = songTitle,
            Artist = artistName
        };

        return music;
    }

    public async Task<FileStreamResult> TryGetFile(string fileName)
    {
        var filePath = Directory.GetFiles(_uploadPath, fileName);

        if (!File.Exists(filePath[0])) return null;

        var fileExtension = Path.GetExtension(filePath[0]).ToLower();
        var mimeType = _allowedMimeTypes.GetValueOrDefault(fileExtension, "application/octet-stream");

        var fileStream = new FileStream(filePath[0], FileMode.Open, FileAccess.Read, FileShare.Read);

        return new FileStreamResult(fileStream, mimeType)
        {
            FileDownloadName = fileName + fileExtension
        };
    }

    public async Task TryDeleteFile(string fileName)
    {
        var filePath = Directory.GetFiles(_uploadPath, fileName);

        if (filePath.Length != 0)
        {
            File.Delete(filePath[0]);
        }

        _logger.LogWarning("File " + fileName + "is not founded");
    }

    public async Task<bool> IsMusicFile(IFormFile musicFile)
    {
        var fileExtension = Path.GetExtension(musicFile.FileName).ToLower();

        return _allowedMimeTypes.ContainsKey(fileExtension) &&
               musicFile.ContentType == _allowedMimeTypes[fileExtension];
    }

    public async Task TryDeleteFiles(List<string> fileNames)
    {
        foreach (var filename in fileNames)
        {
            await TryDeleteFile(filename);
        }
    }
}
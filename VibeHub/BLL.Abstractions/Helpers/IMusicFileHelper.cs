using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BLL.Abstractions.Helpers;

public interface IMusicFileHelper
{
    Task<Music> AddMusicFile(IFormFile musicFile);
    Task<FileStreamResult> TryGetFile(string fileName);
    Task TryDeleteFile(string fileName);
    Task TryDeleteFiles(List<string> fileName);
    Task<bool> IsMusicFile(IFormFile musicFile);
}
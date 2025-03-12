using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BLL.Abstractions.Services;

public interface IMusicService
{
    Task<Music> GetById(Guid id);
    Task<FileStreamResult> GetFileById(Guid id);
    Task<IEnumerable<Music>> AddRange(List<IFormFile> musicList);
    Task DeleteRange(List<Music> musicList);
}
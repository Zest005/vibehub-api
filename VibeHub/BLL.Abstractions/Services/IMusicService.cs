using Core.Errors;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BLL.Abstractions.Services;

public interface IMusicService
{
    Task<EntityResult<Music>> GetById(Guid id);
    Task<EntityResult<FileStreamResult>> GetFileById(Guid id);
    Task<IEnumerable<Music>> AddRange(List<IFormFile> musicList, Guid roomId);
    Task<Music> Delete(Music music);
    Task DeleteRange(List<Music> musicList);
}
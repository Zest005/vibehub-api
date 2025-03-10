using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BLL.Abstractions.Services;

public interface IMusicService
{
    Task<Music> GetById(Guid id);
    Task<FileStreamResult> GetFileById(Guid id);
    Task<IEnumerable<Music>> GetList();
    Task<Music> Add(MusicDto file);
    Task<Music> Update(Guid id, Music music);
    Task<bool> Delete(Guid id);
}
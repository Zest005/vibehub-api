using Core.Models;

namespace BLL.Abstractions.Interfaces;

public interface IMusicService
{
    Task<Music> GetById(Guid id);
    Task<IEnumerable<Music>> GetList();
    Task<Music> Add(Music music);
    Task<Music> Update(Guid id, Music music);
    Task<bool> Delete(Guid id);
}
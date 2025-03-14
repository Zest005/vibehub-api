using Core.Models;

namespace DAL.Abstractions.Interfaces;

public interface IMusicRepository
{
    Task<Music?> GetById(Guid id);
    Task<bool> AddRange(IEnumerable<Music> music);
    Task DeleteRange(List<Music> music);
    Task Delete(Music music);
    Task<bool> Exists(Guid id);
}
using Core.Models;

namespace DAL.Abstractions.Interfaces;

public interface IMusicRepository 
{
    Task<Music> GetById(Guid id);
    Task<IEnumerable<Music>> GetList();
    Task<Music> Add(Music music);
    Task<bool> Delete(Music music);
    Task<bool> Exists(Guid id);
}
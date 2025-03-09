using Core.Models;

namespace BLL.Abstractions.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetList();
    Task<Room> GetById(Guid id);
    Task<Room> Add(Room room);
    Task Update(Guid id, Room room);
    Task Delete(Guid id);
}
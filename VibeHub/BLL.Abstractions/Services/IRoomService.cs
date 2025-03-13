using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Abstractions.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetList();
    Task<Room> GetById(Guid id);
    Task<Room> Create(Guid userId);
    Task Update(Guid id, Room room);
    Task<Room> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicList);
    Task<Room> RemoveMusics(Guid roomId, Guid userId, List<Guid> musicList);
    Task JoinRoom(Guid roomId, Guid userId);
    Task LeaveRoom(Guid roomId, Guid userId);
    Task Delete(Guid id, Guid userId);
}
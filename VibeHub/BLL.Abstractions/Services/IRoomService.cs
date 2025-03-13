using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Abstractions.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetList();
    Task<Room> GetById(Guid id);
    Task<Room> Create(Guid userId);
    Task Update(Guid id, Guid userId, RoomSettings room);
    Task<Room> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicList);
    Task<Room> RemoveMusics(Guid roomId, Guid userId, List<Music> musicList);
    Task JoinRoom(Guid userId, string? password, string? code);
    Task LeaveRoom(Guid roomId, Guid userId);
    Task Delete(Guid id, Guid userId);
}
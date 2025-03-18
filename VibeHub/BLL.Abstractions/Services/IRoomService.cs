using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Abstractions.Services;

public interface IRoomService
{
    Task<IEnumerable<Room>> GetList(int pageNumber, int pageSize);
    Task<Room> GetById(Guid id);
    Task<Room> Create(Guid userId);
    Task Update(Guid id, Guid userId, RoomSettings room);
    Task KickUser(Guid userId, Guid targetUserId, Guid roomId);
    Task<Room> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicList);
    Task<Room> RemoveMusics(Guid roomId, Guid userId, List<RoomsMusicsDto> musicList);
    Task JoinRoom(Guid userId, string? password, string code);
    Task LeaveRoom(Guid roomId, Guid userId);
    Task Delete(Guid id, Guid userId);
}
using Core.DTO;
using Core.Errors;
using Core.Models;
using Microsoft.AspNetCore.Http;

namespace BLL.Abstractions.Services;

public interface IRoomService
{
    Task<EntityResult<IEnumerable<Room>>> GetList(int pageNumber, int pageSize);
    Task<EntityResult<Room>> GetById(Guid id);
    Task<EntityResult<Room>> Create(Guid userId);
    Task<EntityResult<Room>> Update(Guid roomId, Guid userId, RoomSettings room);
    Task<EntityResult<Room>> KickUser(Guid roomId, Guid userId, Guid targetUserId);
    Task<EntityResult<Room>> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicList);
    Task<EntityResult<Room>> RemoveMusics(Guid roomId, Guid userId, List<RoomsMusicsDto> musicList);
    Task<EntityResult<Room>> JoinRoom(Guid userId, string? password, string code);
    Task<EntityResult<Room>> LeaveRoom(Guid roomId, Guid userId);
    Task<EntityResult<Room>> Delete(Guid roomId, Guid userId);
}
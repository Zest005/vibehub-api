using BLL.Abstractions.Helpers;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class RoomService : IRoomService
{
    private readonly IMusicFileHelper _musicFileHelper;
    private readonly ILogger<RoomService> _logger;
    private readonly IMusicService _musicService;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;

    public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository,
        IUserRepository userRepository, IMusicService musicService, IMusicFileHelper musicFileHelper)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _musicService = musicService;
        _musicFileHelper = musicFileHelper;
    }

    public async Task<Room> Create(Guid userId)
    {
        var user = await _userRepository.GetById(userId);

        if (user == null)
        {
            _logger.LogError("User does not exists");
            throw new ArgumentException("User does not exists");
        }

        var room = new Room
        {
            Code = GenerateRoomCode(),
            UserCount = 1,
            OwnerId = userId
        };

        await _roomRepository.Add(room);

        user.Room = room;

        await _userRepository.Update(user);

        return room;
    }

    public async Task<Room> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);

        if (targetUser.Room != null && (targetUser.Room.Id != targetRoom.Id || targetRoom.OwnerId != targetUser.Id))
        {
            _logger.LogError("User is not in the room or he's not owner");
            throw new Exception("User is not in room or he's not owner");
        }

        var playList = await _musicService.AddRange(musicList);

        targetRoom.Playlist.AddRange(playList);

        await _roomRepository.Update(targetRoom);

        return targetRoom;
    }

    public async Task<Room> RemoveMusics(Guid roomId, Guid userId, List<Guid> musicList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);

        if (targetUser.Room != null && (targetUser.Room.Id != targetRoom.Id || targetRoom.OwnerId != targetUser.Id))
        {
            _logger.LogError("User is not in the room or he's not owner");
            throw new Exception("User is not in room or he's not owner");
        }
       
        if (musicList.Any(musicId => targetRoom.Playlist.Any(targetMusic => targetMusic.Id == musicId)))
        {
            targetRoom.Playlist.RemoveAll(targetMusic => 
                musicList.Any(musicId => musicId == targetMusic.Id));
            
            await _roomRepository.Update(targetRoom);
            await _musicFileHelper.TryDeleteFiles(musicList.Select(musicId => musicId + ".*").ToList());

            return targetRoom;
        }

        return targetRoom;
    }

    public async Task JoinRoom(Guid roomId, Guid userId)
    {
        var targetUser = await _userRepository.GetById(userId);
        var targetRoom = await _roomRepository.GetById(roomId);

        if (targetRoom == null || targetUser == null)
        {
            _logger.LogError("Room or user not found");
            throw new Exception("Room or user not found");
        }

        if (targetUser.Room != null && targetUser.Room.Id == roomId)
        {
            _logger.LogError("You're already in the room");
            throw new Exception("You're already in the room");
        }

        targetRoom.UserCount++;
        targetUser.Room = targetRoom;

        await _roomRepository.Update(targetRoom);
        await _userRepository.Update(targetUser);
    }

    public async Task LeaveRoom(Guid roomId, Guid userId)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);

        if (targetRoom == null || targetUser == null)
        {
            _logger.LogError("Room or user not found");
            throw new Exception("Room or user not found");
        }

        if (targetUser.Room == null || targetUser.Room.Id != roomId)
        {
            _logger.LogError("You're not in the room");
            throw new Exception("You're not in the room");
        }

        targetRoom.UserCount--;
        targetUser.Room = null;

        await _roomRepository.Update(targetRoom);
        await _userRepository.Update(targetUser);
    }

    public async Task Delete(Guid id, Guid userId)
    {
        var room = await _roomRepository.GetById(id);
        var user = await _userRepository.GetById(userId);
        if (room == null || user == null || room.OwnerId != user.Id) throw new KeyNotFoundException("Room not found.");

        await _roomRepository.Delete(id);
    }

    public async Task<Room> GetById(Guid id)
    {
        return await _roomRepository.GetById(id);
    }

    public async Task<IEnumerable<Room>> GetList()
    {
        return await _roomRepository.GetList();
    }

    public async Task Update(Guid id, Room room)
    {
        var existingRoom = await _roomRepository.GetById(id);

        if (existingRoom == null) throw new KeyNotFoundException("Room not found.");

        if (room.Id != id && room.Id != Guid.Empty) throw new InvalidOperationException("Room ID cannot be changed.");

        if (room.Code != null && room.Code != existingRoom.Code)
            throw new InvalidOperationException("Code cannot be changed.");

        if (!await _userRepository.Exists(room.OwnerId))
            throw new InvalidOperationException("Owner with such ID does not exist.");

        /*
        if (room.Playlist != null)
        {
            foreach (var musicId in room.Playlist)
            {
                if (!await _musicRepository.Exists(musicId))
                {
                    throw new InvalidOperationException("One or more music IDs do not exist.");
                }
            }
        }
        */

        existingRoom.UserCount = room.UserCount;
        existingRoom.Availability = room.Availability;
        existingRoom.OwnerId = room.OwnerId;
        existingRoom.Playlist = room.Playlist;

        await _roomRepository.Update(existingRoom);
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
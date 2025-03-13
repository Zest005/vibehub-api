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
    private IFilterUtility _filterUtility;
    private IRoomService _roomServiceImplementation;

    public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository,
        IUserRepository userRepository, IMusicService musicService, IMusicFileHelper musicFileHelper, IFilterUtility filterUtility)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _musicService = musicService;
        _musicFileHelper = musicFileHelper;
        _filterUtility = filterUtility;
    }
    public async Task<IEnumerable<Room>> GetList()
    {
        return await _roomRepository.GetList();
    }
    public async Task<Room> GetById(Guid id)
    {
        return await _roomRepository.GetById(id);
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

        if (targetUser == null)
        {
            _logger.LogError("User does not exists");
            throw new ArgumentException("User does not exists");
        }

        if (targetRoom == null)
        {
            _logger.LogError("Room does not exists");
            throw new ArgumentException("Room does not exists");
        }
        
        if (targetUser != null && targetUser.Room.Id != targetRoom.Id)
        {
            _logger.LogError("You are not in the room");
            throw new Exception("You are not in the room");
        }
        
        if (!targetRoom.Settings.AllowUsersUpdateMusic && targetRoom.OwnerId != targetUser.Id)
        {
            _logger.LogError("You cannot update music");
            throw new Exception("You cannot update music");
        }

        var playList = await _musicService.AddRange(musicList);

        targetRoom.Playlist.AddRange(playList);

        await _roomRepository.Update(targetRoom);

        return targetRoom;
    }

    public async Task<Room> RemoveMusics(Guid roomId, Guid userId, List<Music> musicList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);

        if (targetUser == null)
        {
            _logger.LogError("User does not exists");
            throw new ArgumentException("User does not exists");
        }

        if (targetRoom == null)
        {
            _logger.LogError("Room does not exists");
            throw new ArgumentException("Room does not exists");
        }
        
        if (targetUser.Room != null && targetUser.Room.Id != targetRoom.Id)
        {
            _logger.LogError("You are not in the room");
            throw new Exception("You are not in the room");
        }
        
        if (!targetRoom.Settings.AllowUsersUpdateMusic && targetRoom.OwnerId != targetUser.Id)
        {
            _logger.LogError("You cannot update music");
            throw new Exception("You cannot update music");
        }
        
        if (musicList.Any(music => targetRoom.Playlist.Any(targetMusic => targetMusic.Id == music.Id)))
        {
            targetRoom.Playlist.RemoveAll(targetMusic => 
                musicList.Any(music => music.Id == targetMusic.Id));
            
            
            await _roomRepository.Update(targetRoom);
            await _musicFileHelper.TryDeleteFiles(musicList.Select(musicId => musicId + ".*").ToList());

            return targetRoom;
        }

        return targetRoom;
    }

    public async Task JoinRoom(Guid? roomId, Guid userId, string? password, string? code)
    {
        Room? targetRoom = null;
        
        var targetUser = await _userRepository.GetById(userId);
        if (roomId != null)
        {
            targetRoom = await _roomRepository.GetById((Guid)roomId);
        }
        else if (code != null)
        {
            targetRoom = await _roomRepository.GetByCode(code);    
        }
        
        if (targetRoom == null)
        {
            _logger.LogError("Room not found");
            throw new Exception("Room not found");
        }

        if (targetUser.Room != null && targetUser.Room.Id == targetRoom.Id)
        {
            _logger.LogError("You're already in the room");
            throw new Exception("You're already in the room");
        }

        if (targetRoom.Settings.UsersLimit == targetRoom.UserCount)
        {
            _logger.LogError("Room is full");
            throw new Exception("Room is full");
        }

        if (!targetRoom.Settings.Availability &&
            (string.IsNullOrEmpty(password) || password != targetRoom.Settings.Password))
        {
            _logger.LogError("Room is private, access is not allowed");
            throw new Exception("Room is private, access is not allowed");
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

    public async Task Update(Guid id, Guid userId, RoomSettings roomSettings)
    {
        var existingRoom = await _roomRepository.GetById(id);

        if (existingRoom == null)
        {
            _logger.LogError("Room not found");
            throw new KeyNotFoundException("Room not found.");   
        }

        if (existingRoom.OwnerId != userId)
        {
            _logger.LogError("You are not the owner of this room");
            throw new InvalidOperationException("You are not the owner of this room.");
        }
            
        roomSettings = await _filterUtility.Filter(roomSettings);
        existingRoom.Settings = roomSettings;

        await _roomRepository.Update(existingRoom);
    }
    
    public async Task Delete(Guid id, Guid userId)
    {
        var room = await _roomRepository.GetById(id);
        var user = await _userRepository.GetById(userId);
        
        if (room == null || user == null)
            throw new KeyNotFoundException("Room not found.");
        if (room.OwnerId != userId)
            throw new InvalidOperationException("You are not the owner of this room.");

        await _roomRepository.Delete(id);
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
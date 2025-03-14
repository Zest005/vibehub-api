using BLL.Abstractions.Helpers;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
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
    private readonly IGeneratorUtility _generator;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFilterUtility _filterUtility;
    private readonly IGuestRepository _guestRepository;

    public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository,
        IUserRepository userRepository, IMusicService musicService, IMusicFileHelper musicFileHelper,
        IFilterUtility filterUtility, IGeneratorUtility generator, IGuestRepository guestRepository)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _musicService = musicService;
        _musicFileHelper = musicFileHelper;
        _filterUtility = filterUtility;
        _generator = generator;
        _guestRepository = guestRepository;
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
            Code = _generator.GenerateString(5),
            UserCount = 1,
            OwnerId = userId,
            Settings = new RoomSettings()
        };

        await _roomRepository.Add(room);

        user.Room = room;

        await _userRepository.Update(user);

        return room;
    }

    public async Task<Room> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicFilesList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);
        var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

        ValidateUpdateMusicPermission(targetUser, targetGuest, targetRoom);

        var musicList = await _musicService.AddRange(musicFilesList, roomId);

        var playList = musicList.Select(music => new RoomsMusics { RoomId = roomId, MusicId = music.Id }).ToList();

        targetRoom.Playlist.AddRange(playList);

        await _roomRepository.Update(targetRoom);

        return targetRoom;
    }

    public async Task<Room> RemoveMusics(Guid roomId, Guid userId, List<RoomsMusicsDto> musicList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);
        var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

        ValidateUpdateMusicPermission(targetUser, targetGuest, targetRoom);

        if (musicList.Any(music =>
                targetRoom.Playlist.Any(targetMusic =>
                    targetMusic.MusicId == music.MusicId && targetMusic.RoomId == roomId)))
        {
            targetRoom.Playlist.RemoveAll(targetMusic =>
                musicList.Any(music => music.MusicId == targetMusic.MusicId));

            await _roomRepository.Update(targetRoom);
            await _musicService.DeleteRange(musicList.Select(music => new Music
            {
                Id = music.MusicId
            }).ToList());

            await _musicFileHelper.TryDeleteFiles(musicList.Select(musicId => musicId.MusicId + ".*").ToList());

            return targetRoom;
        }

        return targetRoom;
    }

    public async Task KickUser(Guid userId, Guid targetUserId, Guid roomId)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);

        if (targetRoom == null)
        {
            _logger.LogError("Room not found");
            throw new Exception("Room not found");
        }

        if (targetUser == null)
        {
            _logger.LogError("User not found");
            throw new ArgumentException("User not found");
        }

        if (targetUser.Id != targetRoom.OwnerId)
        {
            _logger.LogError("You're not owner of the room");
            throw new ArgumentException("You're not owner of the room");
        }

        if (targetUser.Id == targetUserId)
        {
            _logger.LogError("You can't kick yourself");
            throw new ArgumentException("You can't kick yourself");
        }

        var userToKick = await _userRepository.GetById(targetUserId);

        if (userToKick?.Room?.Id != null && userToKick.Room.Id == targetRoom.Id)
        {
            userToKick.Room = null;
            await _userRepository.Update(userToKick);
        }
        else
        {
            var guestToKick = await _guestRepository.GetById(targetUserId);

            if (guestToKick?.Room?.Id != null && guestToKick.Room.Id == targetRoom.Id)
            {
                guestToKick.Room = null;
                await _guestRepository.Update(guestToKick);
            }
            else
            {
                _logger.LogError("User does not exists in the room");
                throw new ArgumentException("User does not exists in the room");
            }
        }

        targetRoom.UserCount--;

        await _roomRepository.Update(targetRoom);
    }

    public async Task JoinRoom(Guid userId, string? password, string code)
    {
        var targetRoom = await _roomRepository.GetByCode(code);
        var targetUser = await _userRepository.GetById(userId);
        var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

        if (targetUser == null && targetGuest == null)
        {
            _logger.LogError("User does not exists");
            throw new ArgumentException("User does not exists");
        }

        if (targetRoom == null)
        {
            _logger.LogError("Room not found");
            throw new Exception("Room not found");
        }

        if ((targetUser?.Room != null && targetUser.Room?.Id == targetRoom.Id) ||
            (targetGuest?.Room != null && targetGuest.Room?.Id == targetRoom.Id))
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

        if (targetUser != null)
        {
            targetUser.Room = targetRoom;
            await _userRepository.Update(targetUser);
        }
        else
        {
            targetGuest.Room = targetRoom;
            await _guestRepository.Update(targetGuest);
        }

        await _roomRepository.Update(targetRoom);
    }

    public async Task LeaveRoom(Guid roomId, Guid userId)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);
        var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

        if (targetRoom == null)
        {
            _logger.LogError("Room does not exist");
            throw new ArgumentException("Room does not exist");
        }

        if (targetUser == null && targetGuest == null)
        {
            _logger.LogError("User or Guest does not exist");
            throw new ArgumentException("User or Guest does not exist");
        }

        if ((targetUser != null && targetUser.Room?.Id != targetRoom.Id) ||
            (targetGuest != null && targetGuest.Room?.Id != targetRoom.Id))
        {
            _logger.LogError("You are not in the room");
            throw new Exception("You are not in the room");
        }

        targetRoom.UserCount--;

        if (targetUser != null)
        {
            targetUser.Room = null;
            await _userRepository.Update(targetUser);
        }
        else
        {
            targetGuest.Room = null;
            await _guestRepository.Update(targetGuest);
        }

        await _roomRepository.Update(targetRoom);
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

    private void ValidateUpdateMusicPermission(User? user, Guest? guest, Room? room)
    {
        if (user == null && guest == null)
        {
            _logger.LogError("User or Guest does not exist");
            throw new ArgumentException("User or Guest does not exist");
        }

        if (room == null)
        {
            _logger.LogError("Room does not exist");
            throw new ArgumentException("Room does not exist");
        }

        if ((user != null && user.Room?.Id != room.Id) ||
            (guest != null && guest.Room?.Id != room.Id))
        {
            _logger.LogError("You are not in the room");
            throw new Exception("You are not in the room");
        }

        if (!room.Settings.AllowUsersUpdateMusic ||
            (user != null && room.OwnerId != user.Id) ||
            (guest != null && room.OwnerId != guest.Id))
        {
            _logger.LogError("You cannot update music");
            throw new Exception("You cannot update music");
        }
    }
}
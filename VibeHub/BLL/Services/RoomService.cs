using BLL.Abstractions.Helpers;
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Errors;
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

    public async Task<EntityResult<IEnumerable<Room>>> GetList(int pageNumber, int pageSize)
    {
        try
        {
            EntityResult<IEnumerable<Room>> entityResult = new()
            {
                Entity = await _roomRepository.GetList(pageNumber, pageSize)
            };

            return entityResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<IEnumerable<Room>>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> GetById(Guid id)
    {
        try
        {
            EntityResult<Room> entityResult = new()
            {
                Entity = await _roomRepository.GetById(id)
            };

            if (entityResult.Entity == null)
            {
                _logger.LogError("User not found");
                return ErrorCatalog.RoomNotFound;
            }

            return entityResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while logging out user.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> Create(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetById(userId);

            if (user == null)
            {
                _logger.LogError("User does not exists");
                return ErrorCatalog.UserForRoomNotFound;
            }

            var entityResult = new EntityResult<Room>
            {
                Entity = new Room
                {
                    Code = _generator.GenerateString(5),
                    UserCount = 1,
                    OwnerId = userId,
                    Settings = new RoomSettings()
                }
            };

            await _roomRepository.Add(entityResult.Entity);

            user.Room = entityResult.Entity;

            await _userRepository.Update(user);

            return entityResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while creating room.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> AddMusics(Guid roomId, Guid userId, List<IFormFile> musicFilesList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);
        var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

        var result = ValidateUpdateMusicPermission(targetUser, targetGuest, targetRoom);

        if (result.HaveErrors)
            return result;

        var musicList = await _musicService.AddRange(musicFilesList, roomId);

        var playList = musicList.Select(music => new RoomsMusics { RoomId = roomId, MusicId = music.Id }).ToList();

        targetRoom.Playlist.AddRange(playList);

        await _roomRepository.Update(targetRoom);
        result.Entity = targetRoom;

        return result;
    }

    public async Task<EntityResult<Room>> RemoveMusics(Guid roomId, Guid userId, List<RoomsMusicsDto> musicList)
    {
        var targetRoom = await _roomRepository.GetById(roomId);
        var targetUser = await _userRepository.GetById(userId);
        var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

        var result = ValidateUpdateMusicPermission(targetUser, targetGuest, targetRoom);


        if (!string.IsNullOrEmpty(result.Description))
            return result;

        if (musicList.Any(music =>
                result.Entity.Playlist.Any(targetMusic =>
                    targetMusic.MusicId == music.MusicId && targetMusic.RoomId == roomId)))
        {
            result.Entity.Playlist.RemoveAll(targetMusic =>
                musicList.Any(music => music.MusicId == targetMusic.MusicId));

            await _roomRepository.Update(result.Entity);
            await _musicService.DeleteRange(musicList.Select(music => new Music
            {
                Id = music.MusicId
            }).ToList());

            await _musicFileHelper.TryDeleteFiles(musicList.Select(musicId => musicId.MusicId + ".*").ToList());


            return result;
        }

        return result;
    }

    public async Task<EntityResult<Room>> KickUser(Guid roomId, Guid userId, Guid targetUserId)
    {
        try
        {
            var result = new EntityResult<Room>()
            {
                Entity = await _roomRepository.GetById(roomId)
            };

            var targetUser = await _userRepository.GetById(userId);

            if (result.Entity == null)
            {
                _logger.LogError("Room not found");
                return ErrorCatalog.RoomNotFound;
            }

            if (targetUser == null)
            {
                _logger.LogError("User not found");
                return ErrorCatalog.UserForRoomNotFound;
            }

            if (targetUser.Id != result.Entity.OwnerId)
            {
                _logger.LogError("You're not owner of the room");
                return ErrorCatalog.NotUserOwner;
            }

            if (targetUser.Id == targetUserId)
            {
                _logger.LogError("You can't kick yourself");
                return ErrorCatalog.SelfKick;
            }

            var userToKick = await _userRepository.GetById(targetUserId);

            if (userToKick?.Room?.Id != null && userToKick.Room.Id == result.Entity.Id)
            {
                userToKick.Room = null;
                await _userRepository.Update(userToKick);
            }
            else
            {
                var guestToKick = await _guestRepository.GetById(targetUserId);

                if (guestToKick?.Room?.Id != null && guestToKick.Room.Id == result.Entity.Id)
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

            result.Entity.UserCount--;

            await _roomRepository.Update(result.Entity);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while kicking user.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> JoinRoom(Guid userId, string? password, string code)
    {
        try
        {
            var result = new EntityResult<Room>()
            {
                Entity = await _roomRepository.GetByCode(code)
            };

            var targetUser = await _userRepository.GetById(userId);
            var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

            if (targetUser == null && targetGuest == null)
            {
                _logger.LogError("User does not exists");
                return ErrorCatalog.UserForRoomNotFound;
            }

            if (result.Entity == null)
            {
                _logger.LogError("Room not found");
                return ErrorCatalog.RoomNotFound;
            }

            if ((targetUser?.Room != null && targetUser.Room?.Id == result.Entity.Id) ||
                (targetGuest?.Room != null && targetGuest.Room?.Id == result.Entity.Id))
            {
                _logger.LogError("You're already in the room");
                return ErrorCatalog.AlreadyInRoom;
            }

            if (result.Entity.Settings.UsersLimit == result.Entity.UserCount)
            {
                _logger.LogError("Room is full");
                return ErrorCatalog.RoomIsFull;
            }

            if (!result.Entity.Settings.Availability &&
                (string.IsNullOrEmpty(password) || password != result.Entity.Settings.Password))
            {
                _logger.LogError("Room is private, access is not allowed");
                return ErrorCatalog.RoomIsPrivate;
            }

            result.Entity.UserCount++;

            if (targetUser != null)
            {
                targetUser.Room = result.Entity;
                await _userRepository.Update(targetUser);
            }
            else
            {
                targetGuest.Room = result.Entity;
                await _guestRepository.Update(targetGuest);
            }

            await _roomRepository.Update(result.Entity);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while joining room.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> LeaveRoom(Guid roomId, Guid userId)
    {
        try
        {
            var result = new EntityResult<Room>
            {
                Entity = await _roomRepository.GetById(roomId)
            };

            var targetUser = await _userRepository.GetById(userId);
            var targetGuest = targetUser == null ? await _guestRepository.GetById(userId) : null;

            if (result.Entity == null)
            {
                _logger.LogError("Room does not exist");
                return ErrorCatalog.RoomNotFound;
            }

            if (targetUser == null && targetGuest == null)
            {
                _logger.LogError("User or Guest does not exist");
                return ErrorCatalog.UserForRoomNotFound;
            }

            if ((targetUser != null && targetUser.Room?.Id != result.Entity.Id) ||
                (targetGuest != null && targetGuest.Room?.Id != result.Entity.Id))
            {
                _logger.LogError("You are not in the room");
                return ErrorCatalog.NotInRoom;
            }

            result.Entity.UserCount--;

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

            await _roomRepository.Update(result.Entity);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while leaving room.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> Update(Guid id, Guid userId, RoomSettings roomSettings)
    {
        try
        {
            var result = new EntityResult<Room>()
            {
                Entity = await _roomRepository.GetById(id)
            };

            if (result.Entity == null)
            {
                _logger.LogError("Room not found");
                return ErrorCatalog.RoomNotFound;
            }

            if (result.Entity.OwnerId != userId)
            {
                _logger.LogError("You are not the owner of this room");
                return ErrorCatalog.NotUserOwner;
            }

            roomSettings = await _filterUtility.Filter(roomSettings);
            result.Entity.Settings = roomSettings;

            await _roomRepository.Update(result.Entity);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while updating room.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    public async Task<EntityResult<Room>> Delete(Guid id, Guid userId)
    {
        try
        {
            var result = new EntityResult<Room>()
            {
                Entity = await _roomRepository.GetById(id)
            };

            var user = await _userRepository.GetById(userId);

            if (result.Entity == null || user == null)
                return ErrorCatalog.UserForRoomNotFound;

            if (result.Entity.OwnerId != userId)
                return ErrorCatalog.NotUserOwner;

            await _roomRepository.Delete(id);

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while deleting room.");
            return new EntityResult<Room>("Unknown error", true);
        }
    }

    private EntityResult<Room> ValidateUpdateMusicPermission(User? user, Guest? guest, Room? room)
    {
        if (user == null && guest == null)
        {
            _logger.LogError("User or Guest does not exist");
            return ErrorCatalog.UserForRoomNotFound;
        }

        if (room == null)
        {
            _logger.LogError("Room does not exist");
            return ErrorCatalog.RoomNotFound;
        }

        if ((user != null && user.Room?.Id != room.Id) ||
            (guest != null && guest.Room?.Id != room.Id))
        {
            _logger.LogError("You are not in the room");
            return ErrorCatalog.UserNotInRoom;
        }

        if (!room.Settings.AllowUsersUpdateMusic ||
            (user != null && room.OwnerId != user.Id) ||
            (guest != null && room.OwnerId != guest.Id))
        {
            _logger.LogError("You cannot update music");
            return ErrorCatalog.MusicUpdateDenied;
        }

        return new EntityResult<Room>
        {
            Entity = room
        };
    }
}
using BLL.Abstractions.Interfaces;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMusicRepository _musicRepository;
    private readonly IFilterUtility _filterUtility;
    private readonly ILogger<RoomService> _logger;

    public RoomService(IFilterUtility filterUtility, ILogger<RoomService> logger, IRoomRepository roomRepository,
        IUserRepository userRepository, IMusicRepository musicRepository)
    {
        _filterUtility = filterUtility;
        _logger = logger;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _musicRepository = musicRepository;
    }

    public async Task<Room> Add(Room room)
    {
        if (await _roomRepository.Exists(room.Id))
        {
            throw new InvalidOperationException("A room with the same ID already exists.");
        }

        if (!await _userRepository.Exists(room.OwnerId))
        {
            throw new InvalidOperationException("Owner with such ID does not exist.");
        }

        room.Code = GenerateRoomCode();

        await _roomRepository.Add(room);

        return room;
    }

    public async Task Delete(Guid id)
    {
        var room = await _roomRepository.GetById(id);
        if (room == null)
        {
            throw new KeyNotFoundException("Room not found.");
        }

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

        if (existingRoom == null)
        {
            throw new KeyNotFoundException("Room not found.");
        }
        else if (room.Id != id && room.Id != Guid.Empty)
        {
            throw new InvalidOperationException("Room ID cannot be changed.");
        }

        if (room.Code != null && room.Code != existingRoom.Code)
        {
            throw new InvalidOperationException("Code cannot be changed.");
        }

        if (!await _userRepository.Exists(room.OwnerId))
        {
            throw new InvalidOperationException("Owner with such ID does not exist.");
        }

        if (room.MusicIds != null)
        {
            foreach (var musicId in room.MusicIds)
            {
                if (!await _musicRepository.Exists(musicId))
                {
                    throw new InvalidOperationException("One or more music IDs do not exist.");
                }
            }
        }

        existingRoom.UserCount = room.UserCount;
        existingRoom.Availability = room.Availability;
        existingRoom.OwnerId = room.OwnerId;
        existingRoom.MusicIds = room.MusicIds;

        await _roomRepository.Update(existingRoom);
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

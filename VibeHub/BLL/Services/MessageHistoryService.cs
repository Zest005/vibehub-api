using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Errors;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;


namespace BLL.Services;

public class MessageHistoryService : IMessageHistoryService
{
    private readonly IFilterUtility _filterUtility;
    private readonly IMessageHistoryRepository _messageHistoryRepository;
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;
    private readonly ILogger<MessageHistoryService> _logger;


    public MessageHistoryService(IUserService userService, IRoomService roomService,
        IMessageHistoryRepository messageHistoryRepository, IFilterUtility filterUtility,
        ILogger<MessageHistoryService> logger)
    {
        _userService = userService;
        _roomService = roomService;
        _messageHistoryRepository = messageHistoryRepository;
        _filterUtility = filterUtility;
        _logger = logger;
    }

    public async Task<EntityResult<MessageHistory>> Add(MessageDto message)
    {
        try
        {
            var user = await _userService.GetById(message.UserId);
            var room = await _roomService.GetById(message.RoomId);

            if (user == null || room == null)
                return ErrorCatalog.RoomOrUserNotFound;

            message = await _filterUtility.Filter(message);
            if (string.IsNullOrWhiteSpace(message.Text))
                return ErrorCatalog.MessageIsEmpty;

            MessageHistory messageHistory = new()
            {
                UserId = message.UserId,
                RoomId = message.RoomId,
                Message = message.Text
            };

            var isAdded = await _messageHistoryRepository.Add(messageHistory);

            return new EntityResult<MessageHistory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            return new EntityResult<MessageHistory>("Unknown error", true);
        }
    }

    public async Task<EntityResult<IEnumerable<MessageHistory>>> GetList(Guid roomId)
    {
        try
        {
            var targetRoom = await _roomService.GetById(roomId);
            if (targetRoom.Entity == null)
                return ErrorCatalog.MessagesForRoomNotFound;

            var result = new EntityResult<IEnumerable<MessageHistory>>()
            {
                Entity = await _messageHistoryRepository.GetList(roomId)
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred");
            return new EntityResult<IEnumerable<MessageHistory>>("Unknown error", true);
        }
    }
}
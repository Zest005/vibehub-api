using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Models;
using DAL.Abstractions.Interfaces;

namespace BLL.Services;

public class MessageHistoryService : IMessageHistoryService
{
    private readonly IUserService _userService;
    private readonly IRoomService _roomService;
    private readonly IMessageHistoryRepository _messageHistoryRepository;
    private readonly IFilterUtility _filterUtility;
    
    
    public MessageHistoryService(IUserService userService, IRoomService roomService, IMessageHistoryRepository messageHistoryRepository, IFilterUtility filterUtility)
    {
        _userService = userService;
        _roomService = roomService;
        _messageHistoryRepository = messageHistoryRepository;
        _filterUtility = filterUtility;
    }

    public async Task<bool> Add(MessageDto message)
    {
        var user = await _userService.GetById(message.UserId);
        var room = await _roomService.GetById(message.RoomId);
        
        if (user == null || room == null)
        {
            throw new ArgumentNullException("The user and room can't be null");
        }

        message = await _filterUtility.Filter(message);
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        MessageHistory messageHistory = new()
        {
            UserId = message.UserId,
            RoomId = message.RoomId,
            Message = message.Text
        };
        
        var isAdded = await _messageHistoryRepository.Add(messageHistory);

        return isAdded;
    }

    public async Task<List<MessageHistory>> GetList(Guid roomId)
    {
        var room = await _roomService.GetById(roomId);
        if (room == null)
        {
            throw new ArgumentNullException("The room can't be null");
        }
        
        var messages = await _messageHistoryRepository.GetList(roomId);

        return messages;
    }
}
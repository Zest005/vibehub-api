using Core.DTO;
using Core.Models;

namespace BLL.Abstractions.Services;

public interface IMessageHistoryService
{
    Task<bool> Add(MessageDto message);
    Task<List<MessageHistory>> GetList(Guid roomId);
}
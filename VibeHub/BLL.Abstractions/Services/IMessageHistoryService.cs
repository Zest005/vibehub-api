using Core.DTO;
using Core.Errors;
using Core.Models;

namespace BLL.Abstractions.Services;

public interface IMessageHistoryService
{
    Task<EntityResult<MessageHistory>> Add(MessageDto message);
    Task<EntityResult<IEnumerable<MessageHistory>>> GetList(Guid roomId);
}
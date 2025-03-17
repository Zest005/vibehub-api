using Core.Models;

namespace DAL.Abstractions.Interfaces;

public interface IMessageHistoryRepository
{
    Task<bool> Add(MessageHistory messageHistory);
    Task<List<MessageHistory>> GetList(Guid roomId);
}
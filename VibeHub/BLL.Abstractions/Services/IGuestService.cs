using Core.Models;

namespace BLL.Abstractions.Services;

public interface IGuestService
{
    Task<Guest> Create();
    Task Delete(Guid id);
}
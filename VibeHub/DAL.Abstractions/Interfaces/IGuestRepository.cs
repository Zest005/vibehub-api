using Core.Models;

namespace DAL.Abstractions.Interfaces;

public interface IGuestRepository
{
    Task<Guest> Create(Guest guest);
    Task<Guest?> GetById(Guid id);
    Task Update(Guest guest);
    Task Delete(Guest guest);
    Task<bool> Exists(Guid id);
    Task<IEnumerable<Guest>> GetList();
    Task<IEnumerable<Guest>> GetInactiveGuests();
}
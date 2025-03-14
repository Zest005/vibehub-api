using Core.Models;

namespace DAL.Abstractions.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetList();
    Task<User?> GetById(Guid id);
    Task Add(User user);
    Task Update(User user);
    Task Delete(Guid id);
    Task<bool> Exists(Guid id);
    Task<User> GetByEmail(string email);
}
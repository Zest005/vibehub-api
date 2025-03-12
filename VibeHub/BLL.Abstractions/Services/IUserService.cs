using Core.Models;

namespace BLL.Abstractions.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetList();
    Task<User> GetById(Guid id);
    Task Add(User user);
    Task Update(Guid id, User user);
    Task Delete(Guid id);
    Task<User> Authenticate(string email, string password);
    Task Logout(User user);
}
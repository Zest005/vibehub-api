using Core.DTO;
using Core.Errors;
using Core.Models;

namespace BLL.Abstractions.Services;

public interface IUserService
{
    Task<EntityResult<IEnumerable<User>>> GetList();
    Task<EntityResult<User>> GetById(Guid id);
    Task<EntityResult<User>> Add(User user);
    Task<EntityResult<User>> Update(Guid id, User user);
    Task<EntityResult<User>> UpdateDto(Guid id, UserDto userDto);
    Task<EntityResult<User>> Delete(Guid id);
    Task<EntityResult<User>> Authenticate(string email, string password);
    Task<EntityResult<User>> Logout(User user);
    Task<EntityResult<User>?> GetByEmail(string email);
    Task<EntityResult<User>?> GetByNickname(string nickname);
}
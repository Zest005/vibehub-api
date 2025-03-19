using Core.DTO;
using Core.Errors;
using Core.Models;

namespace BLL.Abstractions.Services;

public interface IAuthService
{
    Task<string> Login(LoginDto loginDto);
    Task<EntityResult<User>> Logout(User user);
    Task<EntityResult<User>> Register(RegisterDto registerDto);
}
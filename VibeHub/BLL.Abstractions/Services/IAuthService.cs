using Core.DTO;
using Core.Models;

namespace BLL.Abstractions.Services
{
    public interface IAuthService
    {
        Task<string> Login(LoginDto loginDto);
        Task Logout(User user);
        Task<User> Register(RegisterDto registerDto);
    }
}

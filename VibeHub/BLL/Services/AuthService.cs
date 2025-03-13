using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthService(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        var user = await _userService.Authenticate(loginDto.Email, loginDto.Password);
        if (user == null)
            return null;

        var token = _tokenService.GenerateToken(user);
        await _userService.Update(user.Id, user);
        return token;
    }

    public async Task Logout(User user)
    {
        await _userService.Logout(user);
    }

    public async Task<User> Register(RegisterDto registerDto)
    {
        var user = new User
        {
            Name = registerDto.Name,
            Nickname = registerDto.Nickname,
            Email = registerDto.Email,
            Password = registerDto.Password,
            IsAdmin = false,
            Token = null
        };

        await _userService.Add(user);
        return user;
    }
}
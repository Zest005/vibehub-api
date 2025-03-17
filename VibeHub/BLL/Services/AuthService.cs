using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Errors;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordManagerUtility _passwordManagerUtility;

    public AuthService(IUserService userService, ITokenService tokenService, ILogger<AuthService> logger,
        IPasswordManagerUtility passwordManagerUtility)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
        _passwordManagerUtility = passwordManagerUtility;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        var result = await _userService.Authenticate(loginDto.Email, loginDto.Password);
        
        if (result.Entity == null ||
            !_passwordManagerUtility.VerifyPasswordHash(loginDto.Password, result.Entity.Password, result.Entity.Salt))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", loginDto.Email);
            return null;
        }

        var token = _tokenService.GenerateToken(result.Entity);
        result.Entity.Token = token;
        await _userService.Update(result.Entity.Id, result.Entity);

        return token;
    }

    public async Task Logout(User user)
    {
        user.Token = null;
        await _userService.Update(user.Id, user);
        await _userService.Logout(user);
    }

    public async Task<User> Register(RegisterDto registerDto)
    {
        var existingUserByEmail = await _userService.GetByEmail(registerDto.Email);

        if (existingUserByEmail != null)
        {
            _logger.LogWarning("Email already registered: {Email}", registerDto.Email);
            throw new InvalidOperationException("Email is already registered.");
        }

        var existingUserByNickname = await _userService.GetByNickname(registerDto.Nickname);

        if (existingUserByNickname != null)
        {
            _logger.LogWarning("Nickname already registered: {Nickname}", registerDto.Nickname);
            throw new InvalidOperationException("Nickname is already registered.");
        }

        var (hashedPassword, salt) = _passwordManagerUtility.HashPassword(registerDto.Password);

        var user = new User
        {
            Nickname = registerDto.Nickname,
            Email = registerDto.Email,
            Password = hashedPassword,
            IsAdmin = false,
            Token = null,
            Salt = salt
        };

        await _userService.Add(user);

        return user;
    }
}
using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly ISessionService _sessionService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;
    private readonly IPasswordManagerUtility _passwordManagerUtility;

    public AuthService(IUserService userService, ISessionService sessionService, ILogger<AuthService> logger, IPasswordManagerUtility passwordManagerUtility)
    {
        _userService = userService;
        _sessionService = sessionService;
        _logger = logger;
        _passwordManagerUtility = passwordManagerUtility;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        var user = await _userService.Authenticate(loginDto.Email, loginDto.Password);

        if (user == null || !_passwordManagerUtility.VerifyPasswordHash(loginDto.Password, user.Password, user.Salt))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", loginDto.Email);

            return null;
        }

        if (!string.IsNullOrEmpty(user.SessionId))
        {
            await _sessionService.InvalidateUserSession(user.SessionId);
        }

        var sessionId = _sessionService.CreateSession(user);

        return sessionId;
    }

    public async Task Logout(User user)
    {
        user.SessionId = null;

        await _userService.Update(user.Id, user);
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
            Salt = salt
        };

        await _userService.Add(user);

        return user;
    }
}
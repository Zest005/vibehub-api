using BLL.Abstractions.Services;
using BLL.Abstractions.Utilities;
using Core.DTO;
using Core.Errors;
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
        var result = await _userService.Authenticate(loginDto.Email, loginDto.Password);

        if (result.Entity == null || !_passwordManagerUtility.VerifyPasswordHash(loginDto.Password, result.Entity.Password, result.Entity.Salt))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", loginDto.Email);

            return null;
        }

        if (!string.IsNullOrEmpty(result.Entity.SessionId))
        {
            await _sessionService.InvalidateUserSession(result.Entity.SessionId);
        }

        var sessionId = _sessionService.CreateSession(result.Entity);

        return sessionId;
    }

    public async Task<EntityResult<User>> Logout(User user)
    { 
        user.SessionId = null;

        return await _userService.Update(user.Id, user);
    }

    public async Task<EntityResult<User>> Register(RegisterDto registerDto)
    {
        var result = await _userService.GetByEmail(registerDto.Email);

        if (result.Entity != null)
        {
            _logger.LogWarning("Email already registered: {Email}", registerDto.Email);

            return ErrorCatalog.EmailAlreadyExists;
        }

        result = await _userService.GetByNickname(registerDto.Nickname);

        if (result.Entity != null)
        {
            _logger.LogWarning("Nickname already registered: {Nickname}", registerDto.Nickname);

            return ErrorCatalog.NickNameAlreadyExists;
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

        result = await _userService.Add(user);

        return result;
    }
}
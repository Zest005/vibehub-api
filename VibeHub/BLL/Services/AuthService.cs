using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserService userService, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<string> Login(LoginDto loginDto)
    {
        var user = await _userService.Authenticate(loginDto.Email, loginDto.Password);

        if (user == null || !VerifyPasswordHash(loginDto.Password, user.Password, user.Salt))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", loginDto.Email);
            return null;
        }

        var token = _tokenService.GenerateToken(user);
        user.Token = token;
        await _userService.Update(user.Id, user);
        
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

        var (hashedPassword, salt) = HashPassword(registerDto.Password);

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

    private (string hashedPassword, string salt) HashPassword(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashedPassword = Convert.ToBase64String(hmac.ComputeHash(passwordBytes));

        return (hashedPassword, salt);
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        using var hmac = new HMACSHA512(saltBytes);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var computedHash = Convert.ToBase64String(hmac.ComputeHash(passwordBytes));

        return computedHash == storedHash;
    }
}
using BLL.Abstractions.Services;
using Core.Models;
using DAL.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class SessionService : ISessionService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IGuestRepository _guestRepository;

    public SessionService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<SessionService> logger, IUserRepository userRepository, IGuestRepository guestRepository)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _userRepository = userRepository;
        _guestRepository = guestRepository;
    }

    #region User
    public Guid GetUserIdFromSession()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null");

            throw new InvalidOperationException("HttpContext is null");
        }

        var sessionId = httpContext.Session.GetString("SessionId");

        if (string.IsNullOrEmpty(sessionId))
        {
            _logger.LogWarning("No session ID found in session storage");

            throw new UnauthorizedAccessException("No session ID found");
        }

        var user = _userRepository.GetBySessionId(sessionId).Result;

        if (user == null)
        {
            _logger.LogWarning("Invalid session ID");

            throw new UnauthorizedAccessException("Invalid session ID");
        }

        return user.Id;
    }

    public bool ValidateUserSession(string sessionId)
    {
        var user = _userRepository.GetBySessionId(sessionId).Result;

        if (user == null)
        {
            _logger.LogError("Session validation failed");

            return false;
        }

        return true;
    }

    public async Task InvalidateUserSession(string sessionId)
    {
        var user = await _userRepository.GetBySessionId(sessionId);

        if (user != null)
        {
            user.SessionId = null;
            await _userRepository.Update(user);
        }
    }
    #endregion

    #region Guest
    public Guid GetGuestIdFromSession(string sessionId)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null");

            throw new InvalidOperationException("HttpContext is null");
        }

        var guestIdString = httpContext.Session.Keys.FirstOrDefault(key => httpContext.Session.GetString(key) == sessionId);

        if (string.IsNullOrEmpty(guestIdString) || !Guid.TryParse(guestIdString, out var guestId))
        {
            _logger.LogWarning("Invalid session ID");

            throw new UnauthorizedAccessException("Invalid session ID");
        }

        return guestId;
    }

    public bool ValidateGuestSession(string sessionId)
    {
        var guestIdString = _httpContextAccessor.HttpContext.Session.Keys.FirstOrDefault(key => _httpContextAccessor.HttpContext.Session.GetString(key) == sessionId);

        if (string.IsNullOrEmpty(guestIdString) || !Guid.TryParse(guestIdString, out var guestId))
        {
            _logger.LogError("Guest session validation failed");

            return false;
        }

        return true;
    }

    public async Task InvalidateGuestSession(string sessionId)
    {
        var guestIdString = _httpContextAccessor.HttpContext.Session.Keys.FirstOrDefault(key => _httpContextAccessor.HttpContext.Session.GetString(key) == sessionId);

        if (!string.IsNullOrEmpty(guestIdString) && Guid.TryParse(guestIdString, out var guestId))
        {
            var guest = await _guestRepository.GetById(guestId);

            if (guest != null)
            {
                await _guestRepository.Delete(guest);
            }
        }
    }
    #endregion

    public string CreateSession(User? user = null, Guest? guest = null)
    {
        var sessionId = Guid.NewGuid().ToString();

        if (user != null)
        {
            user.SessionId = sessionId;
            _userRepository.Update(user).Wait();
        }
        else if (guest != null)
        {
            _httpContextAccessor.HttpContext.Session.SetString(guest.Id.ToString(), sessionId);
            guest.LastActive = DateTime.UtcNow;
            _guestRepository.Update(guest).Wait();
        }

        _httpContextAccessor.HttpContext.Session.SetString("SessionId", sessionId);

        return sessionId;
    }
}
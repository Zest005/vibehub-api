using BLL.Abstractions.Services;
using Core.Errors;
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
    public EntityResult<Guid> GetUserIdFromSession()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null");

            return new EntityResult<Guid>("Unknown error, write to support.", true);
        }

        var sessionId = httpContext.Session.GetString("SessionId");

        if (string.IsNullOrEmpty(sessionId))
        {
            _logger.LogWarning("No session ID found in session storage");

            return ErrorCatalog.SessionIdNotFound;
        }

        var user = _userRepository.GetBySessionId(sessionId).Result;

        if (user == null)
        {
            _logger.LogWarning("Invalid session ID");

            return ErrorCatalog.SessionIdInvalid;
        }

        return new EntityResult<Guid>
        {
            Entity = user.Id
        };
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
    public EntityResult<Guid> GetGuestIdFromSession()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null");

            throw new InvalidOperationException("HttpContext is null");
        }

        var sessionId = httpContext.Session.GetString("SessionId");
        
        var guestIdString = httpContext.Session.Keys.FirstOrDefault(key => httpContext.Session.GetString(key) == sessionId);

        if (!string.IsNullOrEmpty(guestIdString))
        {
            var isGuid = Guid.TryParse(guestIdString, out var guestId);

            if (isGuid && _guestRepository.Exists(guestId).Result)
            {
                return new EntityResult<Guid>
                {
                    Entity = guestId
                };
            }
        }

        _logger.LogWarning("Invalid session ID");

        return ErrorCatalog.SessionIdNotFound;
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

    public EntityResult<Guid> GetIdFromVisitor()
    {
        var userResult = GetUserIdFromSession();
        
        if (!userResult.HaveErrors)
        {
            return userResult;   
        }
    
        return GetGuestIdFromSession();
    }
    
    public string CreateSession(User? user = null, Guest? guest = null)
    {
        var sessionId = Guid.NewGuid().ToString();

        if (user != null)
        {
            user.SessionId = sessionId;
            _userRepository.Update(user).Wait();
            _httpContextAccessor.HttpContext.Session.SetString("SessionId", sessionId);
        }
        else if (guest != null)
        {
            _httpContextAccessor.HttpContext.Session.SetString(guest.Id.ToString(), sessionId);
            guest.LastActive = DateTime.UtcNow;
            _guestRepository.Update(guest).Wait();
        }

        return sessionId;
    }
}
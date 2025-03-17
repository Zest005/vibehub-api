using BLL.Abstractions.Services;
using Core.Models;
using DAL.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class SessionService : ISessionService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionService> _logger;
    private readonly AppDbContext _context;

    public SessionService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<SessionService> logger, AppDbContext context)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _context = context;
    }

    public Guid GetIdFromSession()
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

        var user = _context.Users.FirstOrDefault(u => u.SessionId == sessionId);

        if (user == null)
        {
            _logger.LogWarning("Invalid session ID");

            throw new UnauthorizedAccessException("Invalid session ID");
        }

        return user.Id;
    }
    
    public string CreateSession(User? user = null, Guest? guest = null)
    {
        var sessionId = Guid.NewGuid().ToString();

        if (user != null)
        {
            user.SessionId = sessionId;
            _context.Users.Update(user);
            _context.SaveChanges();
        }
        else if (guest != null)
        {
            _httpContextAccessor.HttpContext.Session.SetString(guest.Id.ToString(), sessionId);
        }

        _httpContextAccessor.HttpContext.Session.SetString("SessionId", sessionId);

        return sessionId;
    }

    public bool ValidateSession(string sessionId)
    {
        var user = _context.Users.FirstOrDefault(u => u.SessionId == sessionId);

        if (user == null)
        {
            _logger.LogError("Session validation failed");

            return false;
        }

        return true;
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

    public async Task InvalidateSession(string sessionId)
    {
        var user = _context.Users.FirstOrDefault(u => u.SessionId == sessionId);

        if (user != null)
        {
            user.SessionId = null;
            _context.Users.Update(user);

            await _context.SaveChangesAsync();
        }
    }

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
}
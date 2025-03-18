using Core.Models;

namespace BLL.Abstractions.Services;

public interface ISessionService
{
    string CreateSession(User? user = null, Guest? guest = null);
    Guid GetUserIdFromSession();
    bool ValidateUserSession(string sessionId);
    Task InvalidateUserSession(string sessionId);
    Guid GetGuestIdFromSession(string sessionId);
    bool ValidateGuestSession(string sessionId);
    Task InvalidateGuestSession(string sessionId);
}
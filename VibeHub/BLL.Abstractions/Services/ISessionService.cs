using Core.Models;

namespace BLL.Abstractions.Services;

public interface ISessionService
{
    Guid GetUserIdFromSession();
    string CreateSession(User? user = null, Guest? guest = null);
    bool ValidateUserSession(string sessionId);
    Task InvalidateUserSession(string sessionId);
    bool ValidateGuestSession(string sessionId);
    Task InvalidateGuestSession(string sessionId);
    Guid GetGuestIdFromSession(string sessionId);
}
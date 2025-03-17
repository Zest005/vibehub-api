using Core.Models;

namespace BLL.Abstractions.Services;

public interface ISessionService
{
    Guid GetIdFromSession();
    string CreateSession(User? user = null, Guest? guest = null);
    bool ValidateSession(string sessionId);
    bool ValidateGuestSession(string sessionId);
    Task InvalidateSession(string sessionId);
    Guid GetGuestIdFromSession(string sessionId);
}
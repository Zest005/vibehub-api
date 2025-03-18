using Core.Errors;
using Core.Models;

namespace BLL.Abstractions.Services;

public interface ISessionService
{
    string CreateSession(User? user = null, Guest? guest = null);
    EntityResult<Guid> GetUserIdFromSession();
    bool ValidateUserSession(string sessionId);
    Task InvalidateUserSession(string sessionId);
    EntityResult<Guid> GetGuestIdFromSession();
    bool ValidateGuestSession(string sessionId);
    EntityResult<Guid> GetIdFromVisitor();
}
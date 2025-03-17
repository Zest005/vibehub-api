using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BLL.Abstractions.Services;
using Microsoft.AspNetCore.Http;

public class SessionValidationAttribute : Attribute, IAuthorizationFilter
{
    private readonly ISessionService _sessionService;

    public SessionValidationAttribute(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var sessionId = context.HttpContext.Session.GetString("SessionId");

        if (string.IsNullOrEmpty(sessionId) || (!_sessionService.ValidateSession(sessionId) &&
            !_sessionService.ValidateGuestSession(sessionId)))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}

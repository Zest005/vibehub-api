using BLL.Abstractions.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly ISessionService _sessionService;

    public GuestController(IGuestService guestService, ISessionService sessionService)
    {
        _guestService = guestService;
        _sessionService = sessionService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var guest = await _guestService.Create();
        var sessionId = _sessionService.CreateSession(null, guest);

        HttpContext.Session.SetString("SessionId", sessionId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, sessionId)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new { SessionId = sessionId });
    }

    [Authorize]
    [HttpDelete]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Delete()
    {
        var guestResult = _sessionService.GetGuestIdFromSession();
        if (guestResult.HaveErrors)
            return NotFound();

        await _guestService.Delete(guestResult.Entity);
        
        HttpContext.Session.Remove("SessionId");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }
}
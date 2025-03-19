using System.Security.Claims;
using BLL.Abstractions.Services;
using Core.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly ISessionService _sessionService;

    public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger,
        ISessionService sessionService)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
        _sessionService = sessionService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var sessionId = await _authService.Login(loginDto);

        if (sessionId == null)
        {
            _logger.LogWarning("Unauthorized login attempt for email: {Email}", loginDto.Email);
            return Unauthorized();
        }

        HttpContext.Session.SetString("SessionId", sessionId);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sessionId)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new { SessionId = sessionId });
    }

    [Authorize]
    [HttpPost("logout")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Logout()
    {
        var sessionId = HttpContext.Session.GetString("SessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized();
        }

        var userResult = _sessionService.GetUserIdFromSession();

        if (userResult.HaveErrors)
            return BadRequest(userResult.ToString());

        var result = await _userService.GetById(userResult.Entity);

        if (result.HaveErrors)
            return BadRequest(result.ToString());

        var logoutResult = await _authService.Logout(result.Entity);

        if (logoutResult.HaveErrors)
            return StatusCode(500, logoutResult.ToString());


        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove("SessionId");

        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.Register(registerDto);

        return result.HaveErrors == false
            ? Ok(result.Entity)
            : BadRequest(result.ToString());
    }
}
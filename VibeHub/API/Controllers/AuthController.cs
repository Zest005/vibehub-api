using BLL.Abstractions.Services;
using Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _authService.Login(loginDto);
        if (token == null)
        {
            _logger.LogWarning("Unauthorized login attempt for email: {Email}", loginDto.Email);
            return Unauthorized();
        }

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            _logger.LogWarning("Unauthorized logout attempt");
            return Unauthorized();
        }

        var result = await _userService.GetById(Guid.Parse(userId));
        if (result.Entity == null)
        {
            _logger.LogWarning("User not found for logout: {UserId}", userId);
            return Unauthorized();
        }

        await _authService.Logout(result.Entity);
        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _authService.Register(registerDto);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }
}
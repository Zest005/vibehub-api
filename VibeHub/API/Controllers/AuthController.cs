using BLL.Abstractions.Services;
using Core.Models;
using Core.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _authService.Login(loginDto);
        if (token == null)
            return Unauthorized();

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized();

        await _authService.Logout(user);
        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var user = await _authService.Register(registerDto);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }
}
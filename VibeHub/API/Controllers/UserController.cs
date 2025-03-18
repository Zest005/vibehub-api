using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISessionService _tokenService;

    public UserController(IUserService userService, ISessionService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        try
        {
            var user = await _userService.GetById(id);

            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpPut]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Put([FromForm] UserDto user)
    {
        try
        {
            var userResult = _tokenService.GetUserIdFromSession();
            if (userResult.HaveErrors)
                return Unauthorized();

            await _userService.UpdateDto(userResult.Entity, user);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpDelete]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _userService.Delete(id);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
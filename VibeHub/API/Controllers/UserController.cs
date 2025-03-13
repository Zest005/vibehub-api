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
    private readonly ITokenService _tokenService;

    public UserController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    // GET api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> Get()
    {
        var users = await _userService.GetList();
        return Ok(users);
    }

    // GET api/User/5
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

    // POST api/User
    [HttpPost]
    public async Task<ActionResult<User>> Post([FromBody] User user)
    {
        try
        {
            await _userService.Add(user);
            return CreatedAtAction("Get", new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT api/User/5
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Put([FromForm] UserDto user)
    {
        try
        {
            var userId = _tokenService.GetUserIdFromToken();
            await _userService.Update(userId, user);
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

    // DELETE api/User/5
    [Authorize]
    [HttpDelete]
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
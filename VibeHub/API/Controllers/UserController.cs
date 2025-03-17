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
        var result = await _userService.GetList();

        return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result.ToString());
    }

    // GET api/User/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        try
        {
            var result = await _userService.GetById(id);

            return result.HaveErrors == false ? Ok(result.Entity) : NotFound(new { Description = result.ToString()});
        }
        catch
        {
            return StatusCode(500);
        }
    }

    // POST api/User
    [HttpPost]
    public async Task<ActionResult<User>> Post([FromBody] User user)
    {
        try
        {
            var result = await _userService.Add(user);

            return result.HaveErrors == false
                ? CreatedAtAction("Get", new { id = user.Id }, result.Entity)
                : BadRequest(result.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    // PUT api/User/5
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Put([FromForm] UserDto user)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var result = await _userService.UpdateDto(userId, user);

            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }

    // DELETE api/User/5
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _userService.Delete(id);

            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
}
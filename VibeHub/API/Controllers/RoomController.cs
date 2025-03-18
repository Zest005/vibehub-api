using System.ComponentModel.DataAnnotations;
using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(SessionValidationAttribute))]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private ISessionService _sessionService;
 
    
    public RoomController(IRoomService roomService, ISessionService sessionService)
    {
        _roomService = roomService;
        _sessionService = sessionService;
    }

    // GET api/Room
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> Get()
    {
        try
        {
            var result = await _roomService.GetList();
            
            return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    // GET api/Room/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetById(Guid id)
    {
        var result = await _roomService.GetById(id);
        
        return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result.ToString());
    }

    // POST api/Room
    [Authorize]
    [HttpPost]
    [ServiceFilter(typeof(SessionValidationAttribute), Order = 1)]
    public async Task<ActionResult<Room>> Create()
    {
        try
        {
            var userId = _sessionService.GetUserIdFromSession();
            var result = await _roomService.Create(userId);

            return result.HaveErrors == false
                ? CreatedAtAction("Get", new { id = result.Entity.Id }, result.Entity)
                : BadRequest(result.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPost("{code}/join")]
    public async Task<ActionResult> JoinRoomByCode(string code, [FromForm] string? password)
    {
        try
        {
            var userId = _sessionService.GetUserIdFromSession();
            var result = await _roomService.JoinRoom(userId, password, code);
    
            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPost("{id}/leave")]
    public async Task<ActionResult> LeaveRoom(Guid id)
    {
        try
        {
            var userId = _sessionService.GetIdFromToken();
            var result = await _roomService.LeaveRoom(id, userId);
    
            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPut("{id}/addSongs")]
    public async Task<IActionResult> AddSongs(Guid id, [FromForm] [MinLength(1)] List<IFormFile> files)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var result = await _roomService.AddMusics(id, userId, files);
    
            return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPut("{id}/deleteSongs")]
    public async Task<IActionResult> DeleteSongs(Guid id, [FromBody] [MinLength(1)] List<RoomsMusicsDto> musicList)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var result = await _roomService.RemoveMusics(id, userId, musicList);
    
            return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPut("{roomId}/kick/{targetUserId}")]
    public async Task<IActionResult> Kick(Guid roomId, Guid targetUserId)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var result = await _roomService.KickUser(roomId, userId, targetUserId);
            
            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    // // PUT api/Room/5
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoomSettings roomSettings)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var result = await _roomService.Update(id, userId, roomSettings);
    
            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    // DELETE api/Room/5
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var result = await _roomService.Delete(id, userId);
            
            return result.HaveErrors == false ? NoContent() : BadRequest(result.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
}
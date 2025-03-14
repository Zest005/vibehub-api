using System.ComponentModel.DataAnnotations;
using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ITokenService _tokenService;

    public RoomController(IRoomService roomService, ITokenService tokenService)
    {
        _roomService = roomService;
        _tokenService = tokenService;
    }

    // GET api/Room
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> Get()
    {
        var rooms = await _roomService.GetList();
        return Ok(rooms);
    }

    // GET api/Room/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetById(Guid id)
    {
        var room = await _roomService.GetById(id);
        if (room == null) return NotFound();
        return Ok(room);
    }

    // POST api/Room
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Room>> Create()
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var createdRoom = await _roomService.Create(userId);

            return CreatedAtAction("Get", new { id = createdRoom.Id }, createdRoom);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPost("{code}/join")]
    public async Task<ActionResult> JoinRoomByCode(string code, [FromForm] string? password)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            await _roomService.JoinRoom(userId, password, code);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPost("{id}/leave")]
    public async Task<ActionResult> LeaveRoom(Guid id)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            await _roomService.LeaveRoom(id, userId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPut("{id}/addSongs")]
    public async Task<IActionResult> AddSongs(Guid id, [FromForm] [MinLength(1)] List<IFormFile> files)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var room = await _roomService.AddMusics(id, userId, files);

            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPut("{id}/deleteSongs")]
    public async Task<IActionResult> DeleteSongs(Guid id, [FromBody] [MinLength(1)] List<RoomsMusicsDto> musicList)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            var room = await _roomService.RemoveMusics(id, userId, musicList);

            return Ok(room);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex);
        }
    }

    [Authorize]
    [HttpPut("{roomId}/kick/{targetUserId}")]
    public async Task<IActionResult> Kick(Guid roomId, Guid targetUserId)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            await _roomService.KickUser(userId, targetUserId, roomId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
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
            await _roomService.Update(id, userId, roomSettings);

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

    // DELETE api/Room/5
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = _tokenService.GetIdFromToken();
            await _roomService.Delete(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
using System.ComponentModel.DataAnnotations;
using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ISessionService _sessionService;

    // WebSockets
    private readonly CustomWebSocketHandler _webSocketHandler;

    public RoomController(IRoomService roomService, ISessionService sessionService, CustomWebSocketHandler webSocketHandler)
    {
        _roomService = roomService;
        _sessionService = sessionService;
        _webSocketHandler = webSocketHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> Get([FromQuery] int pageNumber = 1, [Range(1, 50)] [FromQuery] int pageSize = 10)
    {
        var result = await _roomService.GetList(pageNumber, pageSize);
        
        return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result.ToString());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetById(Guid id)
    {
        var roomResult = await _roomService.GetById(id);
        
        return roomResult.HaveErrors == false ? Ok(roomResult.Entity) : NotFound(roomResult.ToString());
    }

    [Authorize]
    [HttpPost]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<ActionResult<Room>> Create()
    {
        try
        {
            var userResult = _sessionService.GetUserIdFromSession();
            
            if (userResult.HaveErrors)
                return BadRequest(userResult.ToString());
            
            var roomResult = await _roomService.Create(userResult.Entity);

            return roomResult.HaveErrors == false
                ? CreatedAtAction("Get", new { id = roomResult.Entity.Id }, roomResult.Entity)
                : BadRequest(roomResult.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("{code}/join")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<ActionResult> JoinRoomByCode(string code, [FromForm] string? password)
    {
        try
        {
            var visitorResult = _sessionService.GetIdFromVisitor();
            
            if (visitorResult.HaveErrors)
                return BadRequest(visitorResult.ToString());
            
            var roomResult = await _roomService.JoinRoom(visitorResult.Entity, password, code);
            
            return roomResult.HaveErrors == false ? Ok(roomResult.Entity) : BadRequest(roomResult.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }

    [HttpPost("{id}/leave")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<ActionResult> LeaveRoom(Guid id)
    {
        try
        {
            var visitorResult = _sessionService.GetIdFromVisitor();
            
            if (visitorResult.HaveErrors)
                return BadRequest(visitorResult.ToString());
            
            var roomResult = await _roomService.LeaveRoom(id, visitorResult.Entity);
    
            return roomResult.HaveErrors == false ? NoContent() : BadRequest(roomResult.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [HttpPut("{id}/addSongs")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> AddSongs(Guid id, [FromForm] [MinLength(1)] List<IFormFile> files)
    {
        try
        {
            var visitorResult = _sessionService.GetIdFromVisitor();
            
            if (visitorResult.HaveErrors)
                return BadRequest(visitorResult.ToString());
            
            var roomResult = await _roomService.AddMusics(id, visitorResult.Entity, files);
    
            return roomResult.HaveErrors == false ? Ok(roomResult.Entity) : NotFound(roomResult.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    [HttpPut("{id}/deleteSongs")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> DeleteSongs(Guid id, [FromBody] [MinLength(1)] List<RoomsMusicsDto> musicList)
    {
        try
        {
            var visitorResult = _sessionService.GetIdFromVisitor();
            
            if (visitorResult.HaveErrors)
                return BadRequest(visitorResult.ToString());
            
            var roomResult = await _roomService.RemoveMusics(id, visitorResult.Entity, musicList);
    
            return roomResult.HaveErrors == false ? Ok(roomResult.Entity) : NotFound(roomResult.ToString());
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPut("{roomId}/kick/{targetUserId}")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Kick(Guid roomId, Guid targetUserId)
    {
        try
        {
            var userResult = _sessionService.GetUserIdFromSession();
            
            if (userResult.HaveErrors)
                return BadRequest(userResult.ToString());
            
            var roomResult = await _roomService.KickUser(roomId, userResult.Entity, targetUserId);
            
            return roomResult.HaveErrors == false ? NoContent() : BadRequest(roomResult.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpPut("{id}")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoomSettings roomSettings)
    {
        try
        {
            var userResult = _sessionService.GetUserIdFromSession();
            var roomResult = await _roomService.Update(id, userResult.Entity, roomSettings);
    
            return roomResult.HaveErrors == false ? NoContent() : BadRequest(roomResult.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userResult = _sessionService.GetUserIdFromSession();
            var roomResult = await _roomService.Delete(id, userResult.Entity);
            
            return roomResult.HaveErrors == false ? NoContent() : BadRequest(roomResult.ToString());
        }
        catch 
        {
            return StatusCode(500);
        }
    }

    #region MUSIC HANDLER TEMPLATE
    [Authorize]
    [HttpPost("{id}/start")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> StartTrack(Guid id)
    {
        var trackPosition = 0; // Get the current track position
        var startTime = DateTime.UtcNow;

        // Store the track state
        // ...

        // Notify all clients about the track start
        var message = new { action = "start", trackPosition, startTime };
        await _webSocketHandler.SendMessageToAllAsync(JsonConvert.SerializeObject(message));

        return Ok();
    }

    [Authorize]
    [HttpPost("{id}/sync")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> SyncTrack(Guid id)
    {
        var trackPosition = 0; // Get the current track position

        // Notify all clients about the current track position
        var message = new { action = "sync", trackPosition };
        await _webSocketHandler.SendMessageToAllAsync(JsonConvert.SerializeObject(message));

        return Ok();
    }
    #endregion
}
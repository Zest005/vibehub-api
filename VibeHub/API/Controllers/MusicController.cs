using BLL.Abstractions.Services;
using Core.DTO;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[ApiController]
[Route("api/music")]
public class MusicController : ControllerBase
{
    private readonly IMusicService _musicService;
    private readonly ILogger<MusicController> _logger;

    public MusicController(IMusicService musicService, ILogger<MusicController> logger)
    {
        _musicService = musicService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var result = await _musicService.GetList();
        
        return result != null ? Ok(result) : NotFound(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool download)
    {
        if (download)
        {
            var result = await _musicService.GetFileById(id);
            
            return result;
        }
        else
        {
            var result = await _musicService.GetById(id);
            
            return result != null ? Ok(result) : NotFound(result);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Add([FromForm] MusicDto music)
    {
        var result = await _musicService.Add(music);
        
        return result != null ? CreatedAtAction(nameof(Add), result) : BadRequest();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _musicService.Delete(id);
        
        return result ? NoContent() : BadRequest(result);
    }
}
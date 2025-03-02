using BLL.Abstractions.Interfaces;
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
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _musicService.GetById(id);
        
        return result != null ? Ok(result) : NotFound(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> Add(Music music)
    {
        var result = await _musicService.Add(music);
        
        return result != null ? CreatedAtAction(nameof(Add), result) : BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Music music)
    {
        var result = await _musicService.Update(id, music);
        
        return result != null ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _musicService.Delete(id);
        
        return result ? NoContent() : BadRequest(result);
    }
}
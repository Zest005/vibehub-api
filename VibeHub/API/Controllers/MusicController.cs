using BLL.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/music")]
public class MusicController : ControllerBase
{
    private readonly ILogger<MusicController> _logger;
    private readonly IMusicService _musicService;

    public MusicController(IMusicService musicService, ILogger<MusicController> logger)
    {
        _musicService = musicService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("{id}")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool download)
    {
        if (download)
        {
            var result = await _musicService.GetFileById(id);

            return result.HaveErrors == false ? result.Entity : NotFound(new { Error = result.ToString() });
        }
        else
        {
            var result = await _musicService.GetById(id);

            return result.HaveErrors == false ? Ok(result.Entity) : NotFound(new { Error = result.ToString() });
        }
    }

    #region MUSIC HANDLER TEMPLATE
    [HttpGet("{id}/stream")]
    [ServiceFilter(typeof(SessionValidationAttribute))]
    public async Task<IActionResult> StreamAudio(Guid id)
    {
        var music = await _musicService.GetById(id);
        if (music.HaveErrors)
            return NotFound();

        var filePath = Path.Combine("path_to_audio_files", $"{music.Entity.Id}.mp3");
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(stream, "audio/mpeg")
        {
            EnableRangeProcessing = true
        };
    }
    #endregion
}
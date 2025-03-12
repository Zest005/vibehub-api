using BLL.Abstractions.Services;
using Core.DTO;
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

    [Authorize]
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

}
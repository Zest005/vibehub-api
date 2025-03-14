using BLL.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly ITokenService _tokenService;

    public GuestController(IGuestService guestService, ITokenService tokenService)
    {
        _guestService = guestService;
        _tokenService = tokenService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var guest = await _guestService.Create();
        var token = _tokenService.GenerateToken(null, guest);

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var guestId = _tokenService.GetIdFromToken();
        await _guestService.Delete(guestId);
        
        return NoContent();
    }

}
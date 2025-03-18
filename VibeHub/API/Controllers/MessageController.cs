using BLL.Abstractions.Services;
using Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(SessionValidationAttribute))]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageHistoryService _messageHistoryService;

    public MessageController(IMessageHistoryService messageHistoryService, ILogger<MessageController> logger)
    {
        _messageHistoryService = messageHistoryService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] Guid roomId)
    {
        var result = await _messageHistoryService.GetList(roomId);

        return result != null ? Ok(result) : NotFound(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] MessageDto message)
    {
        var result = await _messageHistoryService.Add(message);

        return result != null ? Ok(result) : NotFound(result);
    }
}
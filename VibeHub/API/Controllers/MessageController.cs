using BLL.Abstractions.Services;
using Core.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageHistoryService _messageHistoryService;

    public MessageController(IMessageHistoryService messageHistoryService, ILogger<MessageController> logger)
    {
        _messageHistoryService = messageHistoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] Guid roomId)
    {
        var result = await _messageHistoryService.GetList(roomId);

        return result.HaveErrors == false ? Ok(result.Entity) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] MessageDto message)
    {
        var result = await _messageHistoryService.Add(message);

        return result.HaveErrors == false ? Ok(result.Entity) : NotFound(result);
    }
}
using BLL.Abstractions.Services;
using Core.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageHistoryService _messageHistoryService;
    private readonly ILogger<MessageController> _logger;
    
    public MessageController(IMessageHistoryService messageHistoryService, ILogger<MessageController> logger)
    {
        _messageHistoryService = messageHistoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] Guid roomId)
    {
        var result = await _messageHistoryService.GetList(roomId);
        
        return result != null ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] MessageDto message)
    {
        var result = await _messageHistoryService.Add(message);
        
        return result != null ? Ok(result) : NotFound(result);
    }
}
using BLL.Abstractions.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET api/Room
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> Get()
        {
            var rooms = await _roomService.GetList();
            return Ok(rooms);
        }

        // GET api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(Guid id)
        {
            var room = await _roomService.GetById(id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        // POST api/Room
        [HttpPost]
        public async Task<ActionResult<Room>> Post([FromBody] Room room)
        {
            try
            {
                var createdRoom = await _roomService.Add(room);
                return CreatedAtAction("Get", new { id = createdRoom.Id }, createdRoom);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT api/Room/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Room room)
        {
            try
            {
                await _roomService.Update(id, room);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _roomService.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}

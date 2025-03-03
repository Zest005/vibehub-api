using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMusicRepository _musicRepository;

        public RoomController(IRoomRepository roomRepository, IUserRepository userRepository, IMusicRepository musicRepository)
        {
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _musicRepository = musicRepository;
        }

        // GET api/Room
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> Get()
        {
            var rooms = await _roomRepository.GetList();

            return Ok(rooms);
        }

        // GET api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> Get(Guid id)
        {
            var room = await _roomRepository.GetById(id);

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
            if (await _roomRepository.Exists(room.Id))
            {
                return Conflict("A room with the same ID already exists.");
            }

            if (room.Code != null)
            {
                return BadRequest("Code cannot be provided in the request.");
            }

            if (!await _userRepository.Exists(room.OwnerId))
            {
                return BadRequest("Owner with such ID does not exist.");
            }

            if (!await _musicRepository.Exists(room.MusicIds))
            {
                return BadRequest("One or more music IDs do not exist.");
            }

            room.Code = room.Code ?? GenerateRoomCode();

            await _roomRepository.Add(room);

            return CreatedAtAction("Get", new { id = room.Id }, room);
        }

        private static string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        // PUT api/Room/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Room room)
        {
            var existingRoom = await _roomRepository.GetById(id);
            if (existingRoom == null)
            {
                return NotFound();
            }
            else if (room.Id != id && room.Id != Guid.Empty)
            {
                return BadRequest("Room ID cannot be changed.");
            }

            if (room.Code != null && room.Code != existingRoom.Code)
            {
                return BadRequest("Code cannot be changed.");
            }

            if (!await _userRepository.Exists(room.OwnerId))
            {
                return BadRequest("Owner with such ID does not exist.");
            }

            if (!await _musicRepository.ExistsAsync(room.MusicIds))
            {
                return BadRequest("One or more music IDs do not exist.");
            }

            existingRoom.UserCount = room.UserCount;
            existingRoom.Availability = room.Availability;
            existingRoom.OwnerId = room.OwnerId;
            existingRoom.MusicIds = room.MusicIds;

            try
            {
                await _roomRepository.Update(existingRoom);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _roomRepository.Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE api/Room/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var room = await _roomRepository.GetById(id);

            if (room == null)
            {
                return NotFound();
            }

            await _roomRepository.Delete(id);

            return NoContent();
        }
    }
}

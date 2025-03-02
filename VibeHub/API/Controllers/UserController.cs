using Core.Models;
using DAL.Abstractions.Interfaces;
using DAL.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var users = await _userRepository.GetAllAsync();

            return Ok(users);
        }

        // GET api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST api/User
        [HttpPost]
        public async Task<ActionResult<User>> Post([FromBody] User user)
        {
            if (await _userRepository.ExistsAsync(user.Id))
            {
                return Conflict("A user with the same ID already exists.");
            }

            await _userRepository.AddAsync(user);

            return CreatedAtAction("Get", new { id = user.Id }, user);
        }

        // PUT api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }
            else if (user.Id != id && user.Id != Guid.Empty)
            {
                return BadRequest("User ID cannot be changed.");
            }

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Nickname = user.Nickname;
            existingUser.Password = user.Password;
            existingUser.Room = user.Room;

            try
            {
                await _userRepository.UpdateAsync(existingUser);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _userRepository.ExistsAsync(id))
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

        // DELETE api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}

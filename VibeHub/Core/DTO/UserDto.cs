using Microsoft.AspNetCore.Http;

namespace Core.DTO;

public class UserDto
{
    public string? Nickname { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public IFormFile? Avatar { get; set; }
}
using Core.Base;

namespace Core.Models;

public class User : BaseEntity
{
    public string Name { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public string? Token { get; set; }

    public Room? Room { get; set; }
}
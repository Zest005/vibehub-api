using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Core.Base;

namespace Core.Models;

public class User : BaseEntity
{
    [MinLength(3)]
    public string Nickname { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    [MinLength(5)]
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public string? SessionId { get; set; }
    public string? Avatar { get; set; }
    public Room? Room { get; set; }

    public string Salt { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core.Models;

public class RoomSettings
{
    [Key]
    public Guid RoomId { get; set; }

    public short UsersLimit { get; set; } = 10;
    public bool Availability { get; set; } = true;
    public string? Password { get; set; }
    public bool AllowUsersUpdateMusic { get; set; } = true;
    
    [ForeignKey(nameof(RoomId))]
    [JsonIgnore]
    public Room? Room { get; set; } = null!; 
}
using System.Text.Json.Serialization;

namespace Core.Models;

public class RoomsMusics
{
    public Guid MusicId { get; set; }
    [JsonIgnore]
    public Guid RoomId { get; set; }
    
    [JsonIgnore]
    public Room Room { get; set; }
    [JsonIgnore]
    public Music Music { get; set; }
}
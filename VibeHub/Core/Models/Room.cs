using System.Text.Json.Serialization;
using Core.Base;

namespace Core.Models;

public class Room : BaseEntity
{
    public string? Code { get; set; }
    public short UserCount { get; set; }
    public Guid OwnerId { get; set; }

    public RoomSettings Settings { get; set; } = null!;
    public List<Music>? Playlist { get; set; }
}
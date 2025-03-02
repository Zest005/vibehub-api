using Core.Base;

namespace Core.Models;

public class Room : BaseEntity
{
    public string? Code { get; set; }
    public short UserCount { get; set; }
    public bool Availability { get; set; }
    public Guid OwnerId { get; set; }
    public List<Guid>? MusicIds { get; set; }
}
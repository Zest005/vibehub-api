using Core.Base;

namespace Core.Models;

public class Music : BaseEntity
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public Guid RoomId { get; set; }
    
    public Room Room { get; set; } = null!;
}
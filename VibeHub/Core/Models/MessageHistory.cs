using Core.Base;

namespace Core.Models;

public class MessageHistory : BaseEntity
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public Room Room { get; set; }
    public User User { get; set; }
}
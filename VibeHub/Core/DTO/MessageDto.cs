namespace Core.DTO;

public class MessageDto
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; }
}
using Core.Base;

namespace Core.Models;

public class Guest : BaseEntity
{
    public string Name { get; set; }
    public DateTime LastActive { get; set; }

    public Room? Room { get; set; }
}
using System.Text.Json.Serialization;
using Core.Base;

namespace Core.Models;

public class Music : BaseEntity
{
    public string Title { get; set; }
    public string Artist { get; set; }
}
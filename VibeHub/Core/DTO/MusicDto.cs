using Microsoft.AspNetCore.Http;

namespace Core.DTO;

public class MusicDto
{
    public IFormFile File { get; set; }
}
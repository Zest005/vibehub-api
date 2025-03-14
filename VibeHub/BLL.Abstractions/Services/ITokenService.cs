using Core.Models;

namespace BLL.Abstractions.Services;

public interface ITokenService
{
    Guid GetIdFromToken();
    string GenerateToken(User? user = null, Guest? guest = null);
    bool ValidateToken(string token);
}
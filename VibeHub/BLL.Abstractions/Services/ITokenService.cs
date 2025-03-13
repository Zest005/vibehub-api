using Core.Models;

namespace BLL.Abstractions.Services;

public interface ITokenService
{
    Guid GetUserIdFromToken();
    string GenerateToken(User user);
    bool ValidateToken(string token);
}
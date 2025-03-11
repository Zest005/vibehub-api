using Core.DTO;
using Core.Models;

namespace BLL.Abstractions.Services;

public interface ITokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
}
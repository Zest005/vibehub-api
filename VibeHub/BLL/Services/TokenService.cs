using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BLL.Abstractions.Services;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Guid GetIdFromToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null");
            throw new InvalidOperationException("HttpContext is null");
        }

        var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No token found in request headers");
            throw new UnauthorizedAccessException("No token found");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var userIdString = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
        
        if (string.IsNullOrEmpty(userIdString))
        {
            _logger.LogWarning("User ID not found in token");
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogWarning("Invalid User ID format in token");
            throw new UnauthorizedAccessException("Invalid User ID format in token");
        }

        return userId;
    }

    public string GenerateToken(User? user = null, Guest? guest = null)
    {
        if (user is null && guest is null)
        {
            _logger.LogError("Both user and guest are null");
            throw new ArgumentNullException($"{nameof(user)} or {nameof(guest)}", "At least one must be provided.");
        }

        var claims = new List<Claim>();

        if (user is not null)
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        }
        else if (guest is not null)
        {

            claims.Add(new Claim(ClaimTypes.NameIdentifier, guest.Id.ToString()));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured."));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddYears(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return false;
        }
    }
}
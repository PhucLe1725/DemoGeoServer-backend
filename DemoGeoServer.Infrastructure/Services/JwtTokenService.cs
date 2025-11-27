using DemoGeoServer.Application.Interfaces;
using DemoGeoServer.Domain.Entities;
using DemoGeoServer.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DemoGeoServer.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
   _jwtSettings = jwtSettings.Value;
        }

     public string GenerateToken(User user)
        {
       var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
   var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
 {
         new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
         new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Email, user.Email)
  };

     // Add role claim if exists
        if (!string.IsNullOrEmpty(user.Role))
       {
       claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
      audience: _jwtSettings.Audience,
    claims: claims,
         expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: credentials
            );

     return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int? ValidateToken(string token)
    {
            try
    {
        var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

           tokenHandler.ValidateToken(token, new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = true,
           ValidIssuer = _jwtSettings.Issuer,
  ValidateAudience = true,
    ValidAudience = _jwtSettings.Audience,
           ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
  }, out var validatedToken);

   var jwtToken = (JwtSecurityToken)validatedToken;
       var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

            return int.Parse(userId);
          }
  catch
   {
       return null;
        }
        }

        public string GenerateRefreshToken()
        {
      var randomNumber = new byte[64];
         using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
       return Convert.ToBase64String(randomNumber);
        }
    }
}

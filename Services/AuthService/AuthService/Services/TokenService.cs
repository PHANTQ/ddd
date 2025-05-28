using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _dbContext;
        public TokenService(IConfiguration config, ApplicationDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        public async Task<string> CreateAccessToken(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("JWT");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);
            var expiration = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpireMinutes"));

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
        };

            var key = new SymmetricSecurityKey(secretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["ValidIssuer"],
                audience: jwtSettings["ValidAudience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRefreshToken(string userId)
        {
            var refreshToken = Guid.NewGuid().ToString();

            var existingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == userId);
            if (existingToken != null)
            {
                _dbContext.RefreshTokens.Remove(existingToken);
            }
            var newRefreshToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7) 
            };

            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<string> GetUserIdFromRefreshToken(string refreshToken)
        {
            var refreshTokenRecord = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (refreshTokenRecord == null || refreshTokenRecord.ExpiryDate < DateTime.UtcNow)
            {
                return null;
            }

            return refreshTokenRecord.UserId;
        }
    }

}

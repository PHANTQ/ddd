using AuthService.Model;

namespace AuthService.Services
{
    public interface ITokenService
    {
        Task<string> CreateAccessToken(ApplicationUser user);
        Task<string> CreateRefreshToken(string userId);
        Task<string> GetUserIdFromRefreshToken(string refreshToken);
    }

                           //ssssss
}

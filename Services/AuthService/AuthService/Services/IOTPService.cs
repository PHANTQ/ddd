using System.Threading.Tasks;

namespace AuthService.Services
{
    public interface IOTPService
    {
        Task<string> GenerateAndSendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> IsEmailVerified(string email);
    }
}

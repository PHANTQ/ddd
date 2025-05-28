using AuthService.Data;
using AuthService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AuthService.Services
{
    public class OTPService : IOTPService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public OTPService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<string> GenerateAndSendOtpAsync(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            var existing = await _context.EmailOtps.FirstOrDefaultAsync(x => x.Email == email);
            if (existing != null)
                _context.EmailOtps.Remove(existing);

            _context.EmailOtps.Add(new EmailOtp
            {
                Email = email,
                OtpCode = otp,
                ExpiredAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("OtpExpiryMinutes", 5))
            });

            await _context.SaveChangesAsync();

            var smtp = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.Parse(_config["Smtp:Port"]),
                Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
                EnableSsl = true
            };

            var fromAddress = new MailAddress(_config["Smtp:FromEmail"], "No Reply");
            var toAddress = new MailAddress(email);

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Mã OTP",
                Body = $"Mã OTP của bạn là: {otp}",
                IsBodyHtml = false
            };

            await smtp.SendMailAsync(message);
            return otp;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var storedOtp = await _context.EmailOtps
                .FirstOrDefaultAsync(o => o.Email == email && o.OtpCode == otp);

            if (storedOtp == null || storedOtp.ExpiredAt < DateTime.UtcNow)
                return false;

            if (!await _context.VerifiedEmails.AnyAsync(v => v.Email == email))
            {
                _context.VerifiedEmails.Add(new VerifiedEmail
                {
                    Email = email,
                    VerifiedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> IsEmailVerified(string email)
        {
            return await _context.EmailOtps.AnyAsync(x => x.Email == email && x.IsVerified);
        }
    }
}

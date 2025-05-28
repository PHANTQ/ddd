using AuthService.Data;
using AuthService.Helpers;
using AuthService.Model;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using OtpSharp;
using Base32;
namespace AuthService.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOTPService _otpService;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AuthController(
        UserManager<ApplicationUser> userManager,
        IOTPService otpService,
        IConfiguration config,
        ITokenService tokenService,
        ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _otpService = otpService;
            _tokenService = tokenService;
            _config = config;
            _context = context;
            _signInManager = signInManager;
        }

        // Đăng ký người dùng và gửi OTP
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromQuery] string email)
        {
            await _otpService.GenerateAndSendOtpAsync(email);
            return Ok("OTP đã được gửi.");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OTPVerifyModel model)
        {
            var result = await _otpService.VerifyOtpAsync(model.Email, model.Otp);
            if (!result) return BadRequest("OTP không hợp lệ hoặc đã hết hạn.");

            return Ok("OTP hợp lệ.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var isVerified = await _context.VerifiedEmails.AnyAsync(v => v.Email == model.Email);
            if (!isVerified)
                return BadRequest("Email chưa được xác thực OTP.");

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("Email đã được sử dụng.");

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var verified = await _context.VerifiedEmails.FirstOrDefaultAsync(v => v.Email == model.Email);
            if (verified != null)
            {
                _context.VerifiedEmails.Remove(verified);
                await _context.SaveChangesAsync();
            }

            return Ok("Đăng ký thành công.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");

            var accessToken = await _tokenService.CreateAccessToken(user);
            var refreshToken = await _tokenService.CreateRefreshToken(user.Id);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
                return BadRequest("Refresh token không hợp lệ.");

            var userId = await _tokenService.GetUserIdFromRefreshToken(model.RefreshToken);
            if (userId == null)
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("Người dùng không tồn tại.");

            var accessToken = await _tokenService.CreateAccessToken(user);
            var refreshToken = await _tokenService.CreateRefreshToken(userId);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("Người dùng không hợp lệ.");

            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == userId);
            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }

            return Ok("Đăng xuất thành công.");
        }
        [HttpPost("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactorAuth()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("Người dùng không hợp lệ.");

            // Tạo secret và mã hóa Base32
            var key = KeyGeneration.GenerateRandomKey(20);
            var encodedKey = Base32Encoder.Encode(key);

            // Lưu secret vào user (nếu chưa có)
            user.TwoFactorEnabled = true;
            user.TwoFactorSecret = encodedKey;
            await _userManager.UpdateAsync(user);

            // Tạo URI QR Code
            var qrCodeUri = $"otpauth://totp/MyApp:{user.UserName}?secret={encodedKey}&issuer=MyApp";

            return Ok(new { QRCodeUri = qrCodeUri, Secret = encodedKey });
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactorAuth([FromBody] VerifyTwoFactorModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return Unauthorized("Người dùng không hợp lệ hoặc chưa bật 2FA.");

            // Decode lại secret và verify
            var key = Base32Encoder.Decode(user.TwoFactorSecret);
            var totp = new Totp(key);
            var isValid = totp.VerifyTotp(model.Token, out long _);

            if (isValid)
                return Ok("Xác thực thành công.");

            return Unauthorized("Mã OTP không hợp lệ.");
        }

        [HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest("Lỗi đăng nhập Google.");
            var a = 0;
<<<<<<< HEAD
<<<<<<< HEAD
            var K = "npvkkkkk";
=======
            var nqt = "Nguyễn Quốc Thái";
>>>>>>> 9c16ca1e3ae1a38e9880362d244ac7ae4cb34d71
=======
            var nqt = "Nguyễn Quốc Thái";
>>>>>>> origin/main
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                return Ok("Đăng nhập Google thành công.");
            }

            return Unauthorized("Đăng nhập Google thất bại.");
        }

    }
      
    public class OTPVerifyModel
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
    public class VerifyTwoFactorModel
    {
        public string Token { get; set; }
    }


}


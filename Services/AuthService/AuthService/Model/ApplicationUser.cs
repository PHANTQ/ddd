using Microsoft.AspNetCore.Identity;

namespace AuthService.Model
{
    public class ApplicationUser : IdentityUser
    {
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorSecret { get; set; }

    }
}

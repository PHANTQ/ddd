using AuthService.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public DbSet<VerifiedEmail> VerifiedEmails { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}

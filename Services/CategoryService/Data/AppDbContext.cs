using CategoriesService.DTO;
using CategoriesService.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoriesService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<HinhAnhDanhMuc> HinhAnhDanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DanhMuc>()
                .Property(d => d.IDDanhMuc)
                .ValueGeneratedNever();
            modelBuilder.Entity<HinhAnhDanhMuc>()
                .Property(h => h.IDHinhAnhDanhMuc)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<SanPham>()
                .Property(d => d.IDDanhMuc)
                .ValueGeneratedNever();

        }
    }
}

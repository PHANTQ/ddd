using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoriesService.Models
{
    public class SanPham
    {
        [Key]
        public string IDSanPham { get; set; } = default!;
        [ForeignKey("DanhMuc")]
        public int IDDanhMuc { get; set; }

        public string IDCuaHang { get; set; }
        public string TenSanPham { get; set; }
        public string? MoTa { get; set; }
        public string? TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
    }
}

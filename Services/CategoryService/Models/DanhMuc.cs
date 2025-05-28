using System.ComponentModel.DataAnnotations;

namespace CategoriesService.Models
{
    public class DanhMuc
    {
        [Key]
        public int IDDanhMuc { get; set; }
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Độ dài tên danh mục phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Tên danh mục không được chứa ký tự đặc biệt, số, và chỉ chấp nhận ký tự tiếng Việt.")]
        public string TenDanhMuc { get; set; }
        [Required(ErrorMessage = "Cấp danh mục là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Cấp danh mục phải nằm trong khoảng từ 1 đến 5 và không được âm.")]
        public int CapDanhMuc { get; set; }
        public string Path { get; set; }
        public bool TrangThai { get; set; }
        public bool IsLeaf { get; set; }

        // Danh sách hình ảnh liên kết với danh mục này
        public virtual ICollection<HinhAnhDanhMuc> HinhAnhDanhMucs { get; set; }
    }
}

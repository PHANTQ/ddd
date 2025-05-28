using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CategoriesService.Models
{
    public class HinhAnhDanhMuc
    {
        [Key]
        public int IDHinhAnhDanhMuc { get; set; }

        [ForeignKey("DanhMuc")]
        [Required(ErrorMessage = "ID danh mục là bắt buộc.")]
        public int IDDanhMuc { get; set; }
        [Required(ErrorMessage = "Hình ảnh là bắt buộc.")]
        public string HinhAnh { get; set; }


        // Tham chiếu đến danh mục
        public virtual DanhMuc DanhMuc { get; set; }
    }
}

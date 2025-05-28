using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CategoriesService.DTO
{
    public class CreateCategoryLvl2345DTO
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Độ dài tên danh mục phải từ 3 đến 50 ký tự.")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Tên danh mục không được chứa ký tự đặc biệt, số.")]
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string TenDanhMuc { get; set; }
        [Required(ErrorMessage = "Cấp danh mục là bắt buộc.")]
        [Range(2, 5, ErrorMessage = "Cấp danh mục phải nằm trong khoảng từ 2 đến 5 và không được âm.")]
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int CapDanhMuc { get; set; }

        public int IDDanhMuc { get; set; }

        public bool IsLeaf { get; set; }
        public List<ImageDto>? Images { get; set; }
        [DefaultValue(false)]
        public bool ForceOverrideImage { get; set; } = false;
    }
}

namespace CategoriesService.DTO
{
    public class ResponseCategory
    {
        public string TenDanhMuc { get; set; }
        public int IDDanhMuc { set; get; }
    }
    public class ResponseCategoryList
    {
        public int StatusCode { get; set; }
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public List<ResponseCategory>? Categories { get; set; }
    }

}


using CategoriesService.DTO;
using CategoriesService.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CategoriesService.Services
{
    public interface ICategoryService
    {
        Task<ResponseDTO> CreateCategoryLvl1Async(CreateCategoryLvl1DTO dto);
        Task<ResponseCategoryList> GetListCategoryLvl1Async();
        Task<ResponseDTO> CreateCategoryLvl2345Async(CreateCategoryLvl2345DTO dto);
        Task<ResponseDTO> AddImageToCategory(int IDDanhMuc, string hinhAnh);
        Task<ResponseCategoryList> GetListCategoryLvl2345Async(int Socap);
        Task<List<DanhMucShortDTO>> GetDanhMucsByIdAsync(int id);
        Task<List<DanhMucDTO>> GetCategories();
        Task<DanhMucBasicDTO> GetCateByID(int IDDanhMuc);
        Task<List<HinhAnhDanhMuc>> GetImagesByID(int IDDanhMuc);
        Task<ResponseDTO> DeleteAllImages(int IDDanhMuc);
        Task<ResponseDTO> DeleteImagesByID(int imageId);
        Task<ResponseDTO> UpdateStatus(int IDDanhMuc);
        Task<ResponseDTO> UpdateLvl2345(int id, UpdateCategoryLv2345 dTO);
        Task<ResponseDTO> UpdateLvl1(int id, string name);
    }
}

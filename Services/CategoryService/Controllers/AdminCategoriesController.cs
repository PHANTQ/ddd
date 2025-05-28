using CategoriesService.DTO;
using CategoriesService.Models;
using CategoriesService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CategoriesService.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public AdminCategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpPost("CreateCategorieLvl1")]
        public async Task<IActionResult> CreateCategoryLvl1([FromBody] CreateCategoryLvl1DTO dto)
        {
            var response = await _service.CreateCategoryLvl1Async(dto);

            if (response.Status == "success")
            {
                return StatusCode(StatusCodes.Status201Created, response);
            }
            else if (response.Message.Contains("nhạy cảm") || response.Message.Contains("tồn tại"))
            {
                return BadRequest(response);
            }
            else if (response.Message.Contains("quyền"))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, response);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet("GetListCategorieLvl1")]
        public async Task<IActionResult> GetCategoryLevel1()
        {
            var result = await _service.GetListCategoryLvl1Async();

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("CreateCategorieLvl2345")]
        public async Task<IActionResult> CreateCategoryLvl2345([FromBody] CreateCategoryLvl2345DTO dto)
        {
            var response = await _service.CreateCategoryLvl2345Async(dto);

            if (response.Status == "success")
            {
                return StatusCode(StatusCodes.Status201Created, response);
            }
            else if (response.Message.Contains("nhạy cảm") || response.Message.Contains("tồn tại"))
            {
                return BadRequest(response);
            }
            else if (response.Message.Contains("quyền"))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, response);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
                            //aa
        [HttpGet("GetListCategorieLvl2345/{Socap}")]
        public async Task<IActionResult> GetCategoryLevel2345(int Socap)
        {
            var result = await _service.GetListCategoryLvl2345Async(Socap);

            return StatusCode(result.StatusCode, result);
        }
        
        [HttpGet]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> GetCategories()
        {
            var res = await _service.GetCategories();
            if (res == null)
            {
                return BadRequest("Không có danh mục nào được tìm thấy");
            }
            return Ok(res);
        }
        [HttpGet("{id}")]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> GetCateById([FromRoute] int id)
        {
            var res = await _service.GetCateByID(id);
            if (res == null)
            {
                return BadRequest("Không tìm thấy danh mục với ID đã cung cấp");
            }
            return Ok(res);
        }
        [HttpGet("by-id/{id}")]
        public async Task<IActionResult> GetDanhMucById(int id)
        {
            var res = await _service.GetDanhMucsByIdAsync(id);

            if (res == null || res.Count == 0)
                return NotFound("Không tìm thấy danh mục với ID đã cung cấp");

            return Ok(res);
        }

        [HttpGet("{id}/images")]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> GetImages([FromRoute] int id)
        {
            var res = await _service.GetImagesByID(id);
            if (res.Any())
            {
                return Ok(res);
            }
            else
            {
                return BadRequest("Không tìm thấy hình ảnh với ID cung cấp");
            }
        }
        [HttpPost("add")]
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> AddImageByJson([FromBody] HinhAnhDanhMucCreateDTO dto)
        {
            if (dto.HinhAnh == null || dto.HinhAnh.Length == 0)
            {
                return BadRequest("Hình ảnh không hợp lệ.");
            }

            var result = await _service.AddImageToCategory(dto.IDDanhMuc, dto.HinhAnh);

            if (result.Status == "success")
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("{id}/images")]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> DeleteAllImages([FromRoute] int id)
        {
            var res = await _service.DeleteAllImages(id);
            if (res.Status == "success")
            {
                return Ok(res.Message);
            }
            else
            {
                return BadRequest(res.Message);
            }
        }
        [HttpDelete("image/{imageId}")]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> DeleteImagesByID([FromRoute] int imageId)
        {
            var res = await _service.DeleteImagesByID(imageId);
            if (res.Status == "success")
            {
                return Ok(res.Message);
            }
            else
            {
                return BadRequest(res.Message);
            }
        }
        [HttpPatch("{id}/status")]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id)
        {
            var res = await _service.UpdateStatus(id);
            if (res.Status == "success")
            {
                return Ok(res.Message);
            }
            else
            {
                return BadRequest(res.Message);
            }
        }
        [HttpPut("{id}")]
        //[Authorize(Roles ="admin")]
        public async Task<IActionResult> UpdateCateLvl2345([FromRoute] int id, [FromBody] UpdateCategoryLv2345 dto)
        {
            var res = await _service.UpdateLvl2345(id, dto);
            if (res.Status == "success")
            {
                return Ok(res.Message);
            }
            else
            {
                return BadRequest(res.Message);
            }
        }
        [HttpPut("lvl1/{id}")]
        public async Task<IActionResult> UpdateCateLvl1([FromRoute] int id, [FromBody] string tenDanhMuc)
        {
            var res = await _service.UpdateLvl1(id, tenDanhMuc);
            if (res.Status == "success")
            {
                return Ok(res.Message);
            }
            else
            {
                return BadRequest(res.Message);
            }
        }
    }
}

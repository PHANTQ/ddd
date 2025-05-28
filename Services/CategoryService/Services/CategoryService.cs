using CategoriesService.Data;
using CategoriesService.DTO;
using CategoriesService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace CategoriesService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IBadWordService _badWordService;

        public CategoryService(AppDbContext context, IBadWordService badWordService)
        {
            _context = context;
            _badWordService = badWordService;
        }
        public async Task<int> CreateIDDanhMuc()
        {
            var maxId = _context.DanhMucs
                .Select(c => c.IDDanhMuc)
                .AsEnumerable()
                .DefaultIfEmpty(0)
                .Max();


            int nextId = maxId + 1;

            while (await _context.DanhMucs.AnyAsync(c => c.IDDanhMuc == nextId))
            {
                nextId++;
            }

            return nextId;
        }

        public async Task<ResponseDTO> CreateCategoryLvl1Async(CreateCategoryLvl1DTO dto)
        {
            try
            {
                var (isBad, badWords) = await _badWordService.CheckProfanityAsync(dto.TenDanhMuc);
                if (isBad)
                {
                    return new ResponseDTO
                    {
                        Status = "error",
                        Message = $"Tên danh mục chứa từ nhạy cảm: {string.Join(", ", badWords)}"
                    };
                }

                bool categoryExists = await _context.DanhMucs
                    .AnyAsync(c => c.TenDanhMuc.ToLower() == dto.TenDanhMuc.ToLower());

                if (categoryExists)
                {
                    return new ResponseDTO
                    {
                        Status = "error",
                        Message = "Tên danh mục đã tồn tại."
                    };
                }

                int IDDanhMuc = await CreateIDDanhMuc();

                var category = new DanhMuc
                {
                    IDDanhMuc = IDDanhMuc,
                    TenDanhMuc = dto.TenDanhMuc,
                    CapDanhMuc = 1,
                    Path = IDDanhMuc.ToString(),
                    TrangThai = true,
                    IsLeaf = true
                };

                _context.DanhMucs.Add(category);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        Status = "success",
                        Message = "Danh mục đã được thêm thành công."
                    };
                }

                return new ResponseDTO
                {
                    Status = "error",
                    Message = "Có lỗi xảy ra khi thêm danh mục."
                };
            }
            catch (DbUpdateException dbEx)
            {
                return new ResponseDTO
                {
                    Status = "error",
                    Message = "Có lỗi xảy ra khi cập nhật cơ sở dữ liệu: " + dbEx.Message
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Status = "error",
                    Message = "Có lỗi không xác định: " + ex.Message
                };
            }
        }
        public async Task<ResponseCategoryList> GetListCategoryLvl1Async()
        {
            try
            {
                // Giả định bạn có xác thực và đang lấy UserID hoặc quyền từ context
                // bool hasPermission = CheckUserPermission(); // nếu bạn có chức năng phân quyền
                // if (!hasPermission)
                // {
                //     return new ResponseCategoryList
                //     {
                //         StatusCode = 401,
                //         Status = "unauthorized",
                //         Message = "Bạn không có quyền thực hiện thao tác này."
                //     };
                // }

                var categories = await _context.DanhMucs
                    .Where(c => c.CapDanhMuc == 1)
                    .Select(c => new ResponseCategory
                    {
                        TenDanhMuc = c.TenDanhMuc,
                        IDDanhMuc = c.IDDanhMuc
                    })
                    .ToListAsync();

                if (categories == null || !categories.Any())
                {
                    return new ResponseCategoryList
                    {
                        StatusCode = 204,
                        Status = "no_content",
                        Message = "Không có danh mục nào được tìm thấy.",
                        Categories = new List<ResponseCategory>() // tránh null
                    };
                }

                return new ResponseCategoryList
                {
                    StatusCode = 200,
                    Status = "success",
                    Message = "Danh mục đã được lấy thành công.",
                    Categories = categories
                };
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                return new ResponseCategoryList
                {
                    StatusCode = 500,
                    Status = "error",
                    Message = "Đã xảy ra lỗi trong quá trình xử lý: " + ex.Message
                };
            }
        }
        public async Task<ResponseDTO> CreateCategoryLvl2345Async(CreateCategoryLvl2345DTO dto)
        {
            try
            {
                var (isBad, badWords) = await _badWordService.CheckProfanityAsync(dto.TenDanhMuc);
                if (isBad)
                {
                    return new ResponseDTO
                    {
                        Status = "error",
                        Message = $"Tên danh mục chứa từ nhạy cảm: {string.Join(", ", badWords)}"
                    };
                }

                bool categoryExists = await _context.DanhMucs
                    .AnyAsync(c => c.TenDanhMuc.ToLower() == dto.TenDanhMuc.ToLower());

                if (categoryExists)
                {
                    return new ResponseDTO
                    {
                        Status = "error",
                        Message = "Tên danh mục đã tồn tại."
                    };
                }

                bool idCategoryExists = await _context.DanhMucs
                    .AnyAsync(n => n.IDDanhMuc == dto.IDDanhMuc);

                if (!idCategoryExists)
                {
                    return new ResponseDTO
                    {
                        Status = "error",
                        Message = $"Danh mục Cấp {dto.CapDanhMuc} với ID {dto.IDDanhMuc} không tồn tại."
                    };
                }

                bool parentHasImage = await _context.HinhAnhDanhMucs
                    .AnyAsync(img => img.IDDanhMuc == dto.IDDanhMuc);

                if (parentHasImage)
                {
                    if (!dto.ForceOverrideImage)
                    {
                        return new ResponseDTO
                        {
                            Status = "warning",
                            Message = "Danh mục cha đã có ảnh. Nếu tiếp tục thêm danh mục con thì ảnh sẽ bị xóa. Bạn có muốn tiếp tục?"
                        };
                    }
                    else
                    {
                        var imagesToDelete = await _context.HinhAnhDanhMucs
                            .Where(img => img.IDDanhMuc == dto.IDDanhMuc)
                            .ToListAsync();

                        _context.HinhAnhDanhMucs.RemoveRange(imagesToDelete);
                        await _context.SaveChangesAsync();
                    }
                }

                int IDDanhMuc = await CreateIDDanhMuc();

                var parentPath = await _context.DanhMucs
                            .Where(c => c.IDDanhMuc == dto.IDDanhMuc)
                            .Select(c => c.Path)
                            .FirstOrDefaultAsync();

                string categoryPath = $"{parentPath}/{IDDanhMuc}";


                if (dto.CapDanhMuc == 5)
                {
                    dto.IsLeaf = true;
                }
                var category = new DanhMuc
                {
                    IDDanhMuc = IDDanhMuc,
                    TenDanhMuc = dto.TenDanhMuc,
                    CapDanhMuc = dto.CapDanhMuc,
                    Path = categoryPath,
                    TrangThai = true,
                    IsLeaf = dto.IsLeaf,
                };

                _context.DanhMucs.Add(category);
                var result = await _context.SaveChangesAsync();

                if (dto.Images != null && dto.Images.Any())
                {
                    foreach (var img in dto.Images)
                    {
                        _context.HinhAnhDanhMucs.Add(new HinhAnhDanhMuc
                        {
                            IDDanhMuc = IDDanhMuc,
                            HinhAnh = img.HinhAnh
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                var categorydad = await _context.DanhMucs.FindAsync(dto.IDDanhMuc);
                if (categorydad != null)
                {
                    categorydad.IsLeaf = false;
                    _context.DanhMucs.Update(categorydad);
                    await _context.SaveChangesAsync();
                }


                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        Status = "success",
                        Message = "Danh mục đã được thêm thành công."
                    };
                }

                return new ResponseDTO
                {
                    Status = "error",
                    Message = "Có lỗi xảy ra khi thêm danh mục."
                };
            }
            catch (DbUpdateException dbEx)
            {
                return new ResponseDTO
                {
                    Status = "error",
                    Message = "Có lỗi xảy ra khi cập nhật cơ sở dữ liệu: " + dbEx.Message
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Status = "error",
                    Message = "Có lỗi không xác định: " + ex.Message
                };
            }
        }

        public async Task<ResponseCategoryList> GetListCategoryLvl2345Async(int Socap)
        {
            try
            {
                // Giả định bạn có xác thực và đang lấy UserID hoặc quyền từ context
                // bool hasPermission = CheckUserPermission(); // nếu bạn có chức năng phân quyền
                // if (!hasPermission)
                // {
                //     return new ResponseCategoryList
                //     {
                //         StatusCode = 401,
                //         Status = "unauthorized",
                //         Message = "Bạn không có quyền thực hiện thao tác này."
                //     };
                // }

                var categories = await _context.DanhMucs
                    .Where(c => c.CapDanhMuc == Socap)
                    .Select(c => new ResponseCategory
                    {
                        TenDanhMuc = c.TenDanhMuc,
                        IDDanhMuc = c.IDDanhMuc
                    })
                    .ToListAsync();

                if (categories == null || !categories.Any())
                {
                    return new ResponseCategoryList
                    {
                        StatusCode = 204,
                        Status = "no_content",
                        Message = "Không có danh mục nào được tìm thấy.",
                    };
                }

                return new ResponseCategoryList
                {
                    StatusCode = 200,
                    Status = "success",
                    Message = "Danh mục đã được lấy thành công.",
                    Categories = categories
                };
            }
            catch (Exception ex)
            {
                return new ResponseCategoryList
                {
                    StatusCode = 500,
                    Status = "error",
                    Message = "Đã xảy ra lỗi trong quá trình xử lý: " + ex.Message
                };
            }
        }





        private DanhMucDTO BuildDanhMucDTO(DanhMuc danhMuc, List<DanhMuc> allDanhMucs, bool includeChildren = true, bool recursive = true)
        {
            return new DanhMucDTO
            {
                IdDanhMuc = danhMuc.IDDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                CapDanhMuc = danhMuc.CapDanhMuc,
                Path = danhMuc.Path,
                TrangThai = danhMuc.TrangThai,
                IsLeaf = danhMuc.IsLeaf,
                SoLuongSanPhamLienQuan = _context.SanPhams.Count(sp => sp.IDDanhMuc == danhMuc.IDDanhMuc),
                HinhAnhs = danhMuc.HinhAnhDanhMucs.Select(img => new HinhAnhDanhMucDTO
                {
                    IdHinhAnhDanhMuc = img.IDHinhAnhDanhMuc,
                    HinhAnh = img.HinhAnh
                }).ToList(),

                Duongdanpath = BuildDuongDanPath(danhMuc, allDanhMucs),

                Children = includeChildren
                    ? allDanhMucs
                        .Where(child => child.Path != null
                                     && child.Path.StartsWith(danhMuc.Path + "/")
                                     && child.CapDanhMuc == danhMuc.CapDanhMuc + 1)
                        .Select(child => recursive
                            ? BuildDanhMucDTO(child, allDanhMucs, includeChildren: true, recursive: true)
                            : BuildDanhMucDTO(child, allDanhMucs, includeChildren: false)
                        )
                        .ToList()
                    : new List<DanhMucDTO>()
            };
        }
        private List<DanhMucShortDTO> BuildDuongDanPath(DanhMuc danhMuc, List<DanhMuc> allDanhMucs)
        {
            var path = danhMuc.Path ?? "";
            var ids = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
                          .Select(id => int.TryParse(id, out var result) ? result : 0)
                          .Where(id => id > 0)
                          .ToList();

            List<DanhMucShortDTO> root = new();
            List<DanhMucShortDTO> current = root;

            foreach (var id in ids)
            {
                var dm = allDanhMucs.FirstOrDefault(x => x.IDDanhMuc == id);
                if (dm != null)
                {
                    var node = new DanhMucShortDTO
                    {
                        IdDanhMuc = dm.IDDanhMuc,
                        TenDanhMuc = dm.TenDanhMuc
                    };
                    current.Add(node);
                    current = node.Children;
                }
            }

            return root;
        }

        private DanhMucBasicDTO BuildBasicDTO(DanhMuc danhMuc)
        {
            return new DanhMucBasicDTO
            {
                IdDanhMuc = danhMuc.IDDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                CapDanhMuc = danhMuc.CapDanhMuc,
                TrangThai = danhMuc.TrangThai,
                IsLeaf = danhMuc.IsLeaf,
                SoLuongSanPhamLienQuan = _context.SanPhams.Count(sp => sp.IDDanhMuc == danhMuc.IDDanhMuc),
                HinhAnhs = danhMuc.HinhAnhDanhMucs.Select(img => new HinhAnhDanhMucDTO
                {
                    IdHinhAnhDanhMuc = img.IDHinhAnhDanhMuc,
                    HinhAnh = img.HinhAnh
                }).ToList()
            };
        }



        public async Task<List<DanhMucDTO>> GetCategories()
        {
            var allDanhMucs = await _context.DanhMucs
                            .Include(dm => dm.HinhAnhDanhMucs)
                            .ToListAsync();

            var rootDanhMucs = allDanhMucs.Where(dm => dm.CapDanhMuc == 1).ToList();

            var tree = rootDanhMucs.Select(dm => BuildDanhMucDTO(dm, allDanhMucs)).ToList();

            return tree;
        }

        public async Task<DanhMucBasicDTO> GetCateByID(int idDanhMuc)
        {
            var danhMuc = await _context.DanhMucs
                .Include(dm => dm.HinhAnhDanhMucs)
                .FirstOrDefaultAsync(dm => dm.IDDanhMuc == idDanhMuc);

            if (danhMuc == null)
                return null;

            return BuildBasicDTO(danhMuc);
        }



        public async Task<List<DanhMucShortDTO>> GetDanhMucsByIdAsync(int id)
        {
            var allDanhMucs = await _context.DanhMucs.ToListAsync();

            var root = allDanhMucs.FirstOrDefault(dm => dm.IDDanhMuc == id);
            if (root == null)
                return new List<DanhMucShortDTO>();

            var result = new List<DanhMucShortDTO>();

            if (root.CapDanhMuc == 1)
            {
                // Nếu là cấp 1, chỉ cần lấy nó và các con cháu
                var dto = new DanhMucShortDTO
                {
                    IdDanhMuc = root.IDDanhMuc,
                    TenDanhMuc = root.TenDanhMuc,
                    Children = GetChildrenShortDTOs(root, allDanhMucs)
                };
                result.Add(dto);
            }
            else
            {
                var ancestorChain = GetAncestorChain(root, allDanhMucs);
                ancestorChain.Reverse(); // Đảm bảo thứ tự đúng từ gốc đến cha gần nhất

                var current = BuildShortDTO(root, allDanhMucs); // root và con của root

                foreach (var ancestor in ancestorChain)
                {
                    current = new DanhMucShortDTO
                    {
                        IdDanhMuc = ancestor.IDDanhMuc,
                        TenDanhMuc = ancestor.TenDanhMuc,
                        Children = new List<DanhMucShortDTO> { current }
                    };
                }

                result.Add(current);
            }


            return result;
        }

        private DanhMucShortDTO BuildShortDTO(DanhMuc danhMuc, List<DanhMuc> all)
        {
            return new DanhMucShortDTO
            {
                IdDanhMuc = danhMuc.IDDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                Children = GetChildrenShortDTOs(danhMuc, all)
            };
        }

        private List<DanhMucShortDTO> GetChildrenShortDTOs(DanhMuc parent, List<DanhMuc> all)
        {
            return all
                .Where(dm => dm.Path != null
                          && dm.Path.StartsWith(parent.Path + "/")
                          && dm.CapDanhMuc == parent.CapDanhMuc + 1)
                .Select(dm => BuildShortDTO(dm, all))
                .ToList();
        }

        private List<DanhMuc> GetAncestorChain(DanhMuc danhMuc, List<DanhMuc> all)
        {
            var ancestors = new List<DanhMuc>();
            var currentPath = danhMuc.Path;

            while (!string.IsNullOrEmpty(currentPath) && currentPath.Contains("/"))
            {
                var parentPath = currentPath.Substring(0, currentPath.LastIndexOf("/"));
                var parent = all.FirstOrDefault(dm => dm.Path == parentPath);
                if (parent != null)
                {
                    ancestors.Add(parent);
                    currentPath = parent.Path;
                }
                else break;
            }

            ancestors.Reverse(); // 👉 đảm bảo đúng thứ tự từ cấp 1 → cấp N
            return ancestors;
        }




        public async Task<ResponseDTO> AddImageToCategory(int IDDanhMuc, string hinhAnh)
        {
            var danhMuc = await _context.DanhMucs.FirstOrDefaultAsync(x => x.IDDanhMuc == IDDanhMuc);
            if (danhMuc == null)
            {
                return new ResponseDTO
                {
                    Message = "Danh mục không tồn tại.",
                    Status = "error"
                };
            }

            if (danhMuc.CapDanhMuc == 1)
            {
                return new ResponseDTO
                {
                    Message = "Không được thêm hình ảnh cho danh mục cấp 1.",
                    Status = "error"
                };
            }

            var newImage = new HinhAnhDanhMuc
            {
                IDDanhMuc = IDDanhMuc,
                HinhAnh = hinhAnh.ToString()
            };

            _context.HinhAnhDanhMucs.Add(newImage);
            await _context.SaveChangesAsync();

            return new ResponseDTO
            {
                Message = "Thêm hình ảnh thành công.",
                Status = "success"
            };
        }


        public async Task<List<HinhAnhDanhMuc>> GetImagesByID(int IDDanhMuc)
        {
            var images = await _context.HinhAnhDanhMucs.Where(x => x.IDDanhMuc == IDDanhMuc).ToListAsync();
            if (images.Any())
            {
                return images;
            }
            else
            {
                return new List<HinhAnhDanhMuc> { };
            }
        }

        public async Task<ResponseDTO> DeleteAllImages(int IDDanhMuc)
        {
            var images = await _context.HinhAnhDanhMucs.Where(x => x.IDDanhMuc == IDDanhMuc).ToListAsync();
            if (images.Any())
            {
                _context.RemoveRange(images);
                await _context.SaveChangesAsync();
                return new ResponseDTO
                {
                    Message = "Xóa thành công hình ảnh của danh mục.",
                    Status = "success"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    Message = "Không tìm thấy hình ảnh với ID danh mục tương ứng",
                    Status = "error"
                };
            }
        }

        public async Task<ResponseDTO> DeleteImagesByID(int imageId)
        {
            var images = await _context.HinhAnhDanhMucs.FirstOrDefaultAsync(x => x.IDHinhAnhDanhMuc == imageId);
            Console.WriteLine(imageId);
            if (images != null)
            {
                _context.Remove(images);
                await _context.SaveChangesAsync();
                return new ResponseDTO
                {
                    Message = "Xóa thành công hình ảnh của danh mục.",
                    Status = "success"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    Message = "Không tìm thấy hình ảnh với ID hình ảnh tương ứng",
                    Status = "error"
                };
            }
        }
        public async Task<ResponseDTO> UpdateStatus(int IDDanhMuc)
        {
            var res = await _context.DanhMucs.FirstOrDefaultAsync(x => x.IDDanhMuc == IDDanhMuc);
            if (res != null)
            {
                res.TrangThai = !res.TrangThai;
                _context.SaveChanges();
                return new ResponseDTO
                {
                    Message = "Trạng thái danh mục đã được cập nhật thành công.",
                    Status = "success"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    Message = "Không tìm thấy danh mục với ID tương ứng",
                    Status = "error"
                };
            }
        }

        public async Task<ResponseDTO> UpdateLvl2345(int id, UpdateCategoryLv2345 dTO)
        {
            var exists = await _context.DanhMucs.FirstOrDefaultAsync(x => x.IDDanhMuc == id);
            if (exists == null)
            {
                return new ResponseDTO
                {
                    Message = "Không tìm thấy danh mục với ID tương ứng",
                    Status = "error"
                };
            }

            if (dTO.IDDanhMuc == id)
            {
                return new ResponseDTO
                {
                    Message = "Không thể chọn chính danh mục đang cập nhật làm danh mục cha.",
                    Status = "error"
                };
            }

            if (!exists.IsLeaf)
            {
                return new ResponseDTO
                {
                    Message = "Không thể cập nhật danh mục có danh mục con.",
                    Status = "error"
                };
            }

            // ❗ RÀNG BUỘC: Cấp 2 không được cập nhật lên cấp 4,5,...
            if (dTO.CapDanhMuc > exists.CapDanhMuc + 1)
            {
                return new ResponseDTO
                {
                    Message = $"Không thể tăng cấp danh mục quá 1 cấp. Hiện tại cấp {exists.CapDanhMuc}, chỉ được phép lên cấp {exists.CapDanhMuc + 1}.",
                    Status = "error"
                };
            }


            // ❗ RÀNG BUỘC: Trùng tên trong cùng cấp
            var isNameExists = await _context.DanhMucs
                .AnyAsync(x => x.IDDanhMuc != id && x.CapDanhMuc == dTO.CapDanhMuc && x.TenDanhMuc == dTO.TenDanhMuc);
            if (isNameExists)
            {
                return new ResponseDTO
                {
                    Message = "Tên danh mục này đã tồn tại trong cùng cấp.",
                    Status = "error"
                };
            }

            // Lưu path cũ để xử lý cập nhật cha cũ
            string oldPath = exists.Path;

            // Xác định ID cha cũ từ path
            int? oldParentId = null;
            if (!string.IsNullOrEmpty(oldPath))
            {
                var parts = oldPath.Split('/');
                if (parts.Length >= 2 && int.TryParse(parts[parts.Length - 2], out int parsedId))
                {
                    oldParentId = parsedId;
                }
            }

            // Kiểm tra giảm cấp không quá 1 cấp
            if (dTO.CapDanhMuc < exists.CapDanhMuc)
            {
                var levelDiff = exists.CapDanhMuc - dTO.CapDanhMuc;
                if (levelDiff > 1)
                {
                    return new ResponseDTO
                    {
                        Message = $"Không thể giảm cấp quá 1 cấp. Danh mục hiện tại là cấp {exists.CapDanhMuc}, chỉ được phép xuống cấp {exists.CapDanhMuc - 1}.",
                        Status = "error"
                    };
                }
            }

            // Xác định danh mục cha mới
            DanhMuc newParent = null;
            if (dTO.IDDanhMuc == 0)
            {
                if (dTO.CapDanhMuc != 1)
                {
                    return new ResponseDTO
                    {
                        Message = "Danh mục không có cha (IDDanhMuc = 0) phải có cấp là 1.",
                        Status = "error"
                    };
                }
            }
            else
            {
                newParent = await _context.DanhMucs.FirstOrDefaultAsync(x => x.IDDanhMuc == dTO.IDDanhMuc);
                if (newParent == null)
                {
                    return new ResponseDTO
                    {
                        Message = "Không tìm thấy danh mục cha mới.",
                        Status = "error"
                    };
                }

                if (newParent.CapDanhMuc != dTO.CapDanhMuc - 1)
                {
                    return new ResponseDTO
                    {
                        Message = $"Để cập nhật danh mục cấp {dTO.CapDanhMuc}, cần chọn danh mục cha có cấp là {dTO.CapDanhMuc - 1}.",
                        Status = "error"
                    };
                }
            }

            // Kiểm tra có thay đổi cha không
            bool isParentChanged = false;
            if (dTO.CapDanhMuc > 1 && newParent != null)
            {
                if (oldParentId.HasValue && newParent.IDDanhMuc != oldParentId.Value)
                {
                    isParentChanged = true;
                }
                else if (!oldParentId.HasValue)
                {
                    isParentChanged = true;
                }
            }

            // Cập nhật thông tin danh mục
            exists.TenDanhMuc = dTO.TenDanhMuc;
            exists.CapDanhMuc = dTO.CapDanhMuc;
            exists.Path = dTO.CapDanhMuc == 1
                ? $"{id}"
                : $"{newParent?.Path}/{id}";

            // Nếu cha thay đổi, cập nhật IsLeaf cho cha mới và cha cũ
            if (isParentChanged)
            {
                // Cập nhật IsLeaf cho cha mới
                if (newParent != null && newParent.IsLeaf)
                {
                    newParent.IsLeaf = false;
                }

                // Cập nhật IsLeaf cho cha cũ dựa trên path cũ
                if (!string.IsNullOrEmpty(oldPath))
                {
                    var oldPathParts = oldPath.Split('/');
                    if (oldPathParts.Length >= 2 && int.TryParse(oldPathParts[oldPathParts.Length - 2], out int oldParentDanhMucId))
                    {
                        var oldParent = await _context.DanhMucs.FirstOrDefaultAsync(x => x.IDDanhMuc == oldParentDanhMucId);
                        if (oldParent != null)
                        {
                            int expectedSlashCount = oldParent.Path.Count(c => c == '/') + 1;

                            var children = await _context.DanhMucs
                                .Where(dm => dm.IDDanhMuc != id &&
                                             dm.Path != null &&
                                             dm.Path.StartsWith(oldParent.Path + "/"))
                                .ToListAsync(); // thực thi ở DB trước

                            var childrenStillExist = children
                                .Where(dm => dm.Path.Count(c => c == '/') == expectedSlashCount)
                                .Any();

                            oldParent.IsLeaf = !childrenStillExist;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new ResponseDTO
            {
                Message = "Cập nhật danh mục thành công.",
                Status = "success"
            };
        }





        public async Task<ResponseDTO> UpdateLvl1(int id, string name)
        {
            var exists = await _context.DanhMucs.FirstOrDefaultAsync(x => x.IDDanhMuc == id);
            if (exists == null)
            {
                return new ResponseDTO
                {
                    Message = "Không tìm thấy danh mục với ID tương ứng",
                    Status = "error"
                };
            }

            // Ràng buộc: Chỉ được phép thay đổi tên danh mục cấp 1
            if (exists.CapDanhMuc != 1)
            {
                return new ResponseDTO
                {
                    Message = "Chỉ được phép thay đổi tên danh mục cấp 1. Các cấp khác không được thay đổi tên.",
                    Status = "error"
                };
            }

            // Ràng buộc: Tên danh mục cấp 1 không được trùng
            bool isNameExists = await _context.DanhMucs
                .AnyAsync(x => x.IDDanhMuc != id && x.CapDanhMuc == 1 && x.TenDanhMuc == name);
            if (isNameExists)
            {
                return new ResponseDTO
                {
                    Message = "Tên danh mục cấp 1 đã tồn tại.",
                    Status = "error"
                };
            }

            exists.TenDanhMuc = name;

            var validationContext = new ValidationContext(exists);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(exists, validationContext, validationResults, true);

            if (!isValid)
            {
                var message = string.Join("; ", validationResults.Select(v => v.ErrorMessage));
                return new ResponseDTO
                {
                    Message = $"Dữ liệu không hợp lệ: {message}",
                    Status = "error"
                };
            }

            await _context.SaveChangesAsync();

            return new ResponseDTO
            {
                Message = "Cập nhật danh mục thành công",
                Status = "success"
            };
        }


    }
}

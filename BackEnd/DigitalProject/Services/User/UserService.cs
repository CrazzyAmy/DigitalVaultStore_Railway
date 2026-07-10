// Services/User/UserService.cs
using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Exceptions;
using DigitalProject.Interface;
using DigitalProject.Interface.Role;
using DigitalProject.Interface.User;
using DigitalProject.Request;
using DigitalProject.Response;
using DigitalProject.Security;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly DigitalVaultStoreDbContext _dbcontext;
        private readonly IRoleRepository _roleRepository;  

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            DigitalVaultStoreDbContext dbcontext,
            IRoleRepository roleRepository)  
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _dbcontext = dbcontext;
            _roleRepository = roleRepository;
        }

        // ── 前台 ──────────────────────────────────────────────

        public async Task UpdateDisplayNameAsync(Guid userId, UpdateDisplayNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
                throw new AppException("顯示名稱不可為空", 400);

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new AppException("找不到使用者", 404);

            await _userRepository.UpdateDisplayNameAsync(userId, request.DisplayName);
        }

        public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new AppException("找不到使用者", 404);

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash!))
                throw new AppException("目前密碼錯誤", 401);

            if (request.NewPassword.Length < 8)
                throw new AppException("新密碼至少需要 8 個字元", 400);

            var newHash = _passwordHasher.Hash(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, newHash);
        }

        public async Task<List<PurchaseResponse>> GetPurchasesAsync(Guid userId)
        {
            var orderItems = await _dbcontext.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Order)
                .Where(oi =>
                    oi.Order.UserId == userId &&
                    (oi.Order.Status == OrderStatus.Paid ||
                     oi.Order.Status == OrderStatus.Completed))
                .ToListAsync();

            return orderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => g.OrderByDescending(oi => oi.Order.CreatedAt).First())
                .Select(oi => new PurchaseResponse
                {
                    ProductId = oi.ProductId,
                    Name = oi.Product.Name,
                    Price = oi.Product.Price,
                    ThumbnailUrl = string.IsNullOrEmpty(oi.Product.ThumbnailUrl)
                        ? $"https://picsum.photos/400/220?random={oi.ProductId}"
                        : oi.Product.ThumbnailUrl,
                    DownloadUrl = oi.Product.DownloadUrl,
                    CategoryName = oi.Product.Category.Name,
                    PurchasedAt = oi.Order.CreatedAt
                })
                .ToList();
        }

        public async Task<string> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            // 1. 驗證檔案類型
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new AppException("僅接受 JPG、PNG、WebP 格式", 400);

            // 2. 驗證檔案大小（2MB）
            if (file.Length > 2 * 1024 * 1024)
                throw new AppException("檔案大小不能超過 2MB", 400);

            // 3. 產生唯一檔名
            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

            // 4. 確認資料夾存在
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // 5. 儲存檔案
            var filePath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 6. 刪除舊頭貼（如果有）
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldFileName = Path.GetFileName(user.AvatarUrl);
                var oldFilePath = Path.Combine(folder, oldFileName);
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            // 7. 更新資料庫
            var avatarUrl = $"/uploads/avatars/{fileName}";
            await _userRepository.UpdateAvatarAsync(userId, avatarUrl);

            return avatarUrl;
        }

        // ── 後台 ──────────────────────────────────────────────

        public async Task<IEnumerable<AdminUserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToAdminResponse);
        }

        public async Task<AdminUserResponse?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);
            return MapToAdminResponse(user);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);
            if (!user.IsActive)
                throw new AppException("此帳號已停用");

            await _userRepository.DeactivateAsync(id);
        }

        public async Task ActivateAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);
            if (user.IsActive)
                throw new AppException("此帳號已啟用");

            await _userRepository.ActivateAsync(id);
        }

        public async Task UpdateRoleAsync(Guid id, UpdateUserRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new AppException("使用者不存在", 404);

            var role = await _roleRepository.GetByCodeAsync(request.RoleCode);
            if (role == null)
                throw new AppException("角色不存在", 404);

            await _userRepository.UpdateRoleAsync(id, role.Id);
        }

        // ── MapToAdminResponse ─────────────────────────────────
        private static AdminUserResponse MapToAdminResponse(Models.User u) => new()
        {
            Id = u.Id,
            Email = u.Email,
            DisplayName = u.DisplayName,
            AvatarUrl = u.AvatarUrl,
            IsActive = u.IsActive,
            Provider = u.Provider.ToString(),
            CreatedAt = u.CreatedAt,
            Roles = u.UserRoles.Select(ur => ur.Role.Code).ToList()
        };

        public async Task<PagedResponse<AdminUserResponse>> GetAllAsync(PagedRequest request)
        {
            var paged = await _userRepository.GetAllPagedAsync(request);
            return new PagedResponse<AdminUserResponse>
            {
                Data = paged.Data.Select(MapToAdminResponse).ToList(),
                Total = paged.Total,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }
    }
}
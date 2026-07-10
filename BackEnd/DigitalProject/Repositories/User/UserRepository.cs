// Repositories/UserRepository.cs
using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Interface.User;
using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DigitalVaultStoreDbContext _dbcontext;

        public UserRepository(DigitalVaultStoreDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task CreateAsync(User user)
        {
            await _dbcontext.Users.AddAsync(user);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email) =>
            await _dbcontext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByIdAsync(Guid id) =>
            await _dbcontext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<bool> IsEmailExistsAsync(string email) =>
            await _dbcontext.Users
                .AnyAsync(u => u.Email == email);

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken) =>
            await _dbcontext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        // ✅ 補上 Include
        public async Task<User?> GetByProviderKeyAsync(string providerKey) =>
            await _dbcontext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.Provider == AuthProvider.Google &&
                    u.ProviderKey == providerKey);

        public async Task UpdateDisplayNameAsync(Guid id, string displayName)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.DisplayName = displayName;
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdatePasswordAsync(Guid id, string passwordHash)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.PasswordHash = passwordHash;
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenAsync(User user)
        {
            _dbcontext.Users.Update(user);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _dbcontext.Users.Update(user);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task AddRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            };
            await _dbcontext.UserRoles.AddAsync(userRole);
            await _dbcontext.SaveChangesAsync();
        }

        // 查所有使用者
        public async Task<List<User>> GetAllAsync() =>
            await _dbcontext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

        // 停用帳號
        public async Task DeactivateAsync(Guid id)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.IsActive = false;
            await _dbcontext.SaveChangesAsync();
        }

        // 啟用帳號
        public async Task ActivateAsync(Guid id)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.IsActive = true;
            await _dbcontext.SaveChangesAsync();
        }

        // 修改角色
        public async Task UpdateRoleAsync(Guid userId, Guid roleId)
        {
            // 先移除所有舊角色
            var existing = await _dbcontext.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
            _dbcontext.UserRoles.RemoveRange(existing);

            // 新增新角色
            await _dbcontext.UserRoles.AddAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });

            await _dbcontext.SaveChangesAsync();
        }
        public async Task UpdateAvatarAsync(Guid id, string avatarUrl)
        {
            var user = await _dbcontext.Users.FindAsync(id);
            if (user == null) return;
            user.AvatarUrl = avatarUrl;
            await _dbcontext.SaveChangesAsync();
        }

        public async Task<PagedResponse<User?>> GetAllPagedAsync(PagedRequest request)
        {
            var queryable = _dbcontext.Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .AsQueryable(); 
            var total = await queryable.CountAsync();
            var data = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            return new PagedResponse<User?>
            {
                Data = data,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
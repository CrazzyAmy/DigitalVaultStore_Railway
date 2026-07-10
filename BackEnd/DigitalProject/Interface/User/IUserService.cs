using DigitalProject.Request;
using DigitalProject.Response;
using System.Threading.Tasks;

namespace DigitalProject.Interface.User
{
    public interface IUserService
    {
        // 前台
        Task UpdateDisplayNameAsync(Guid userId, UpdateDisplayNameRequest request);
        Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request);
        Task<List<PurchaseResponse>> GetPurchasesAsync(Guid userId);
        Task<string> UploadAvatarAsync(Guid userId, IFormFile file);
        Task<PagedResponse<AdminUserResponse>> GetAllAsync(PagedRequest request);

        // 後台
        Task<IEnumerable<AdminUserResponse>> GetAllAsync();
        Task<AdminUserResponse?> GetByIdAsync(Guid id);
        Task DeactivateAsync(Guid id);
        Task ActivateAsync(Guid id);
        Task UpdateRoleAsync(Guid id, UpdateUserRoleRequest request);
    }
}

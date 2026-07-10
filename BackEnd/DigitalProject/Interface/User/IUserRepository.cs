using DigitalProject.Request;
using Microsoft.AspNetCore.Mvc;
using DigitalProject.Response;
using DigitalProject.Models;


namespace DigitalProject.Interface.User
{
    public interface IUserRepository
    {
        Task<Models.User?> GetByEmailAsync(string email);
        Task CreateAsync(Models.User user);
        Task<Models.User?> GetByIdAsync(Guid id);
        Task<Models.User?> GetByProviderKeyAsync(string providerKey);
        Task<bool> IsEmailExistsAsync(string email);
        Task UpdateDisplayNameAsync(Guid id, string displayName);
        Task UpdatePasswordAsync(Guid id, string passwordHash);
        Task UpdateRefreshTokenAsync(Models.User user);
        Task<Models.User?> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateAsync(Models.User user);
        Task AddRoleAsync(Guid userId, Guid roleId);
        Task<List<Models.User?>> GetAllAsync();                  
        Task DeactivateAsync(Guid id);                          
        Task ActivateAsync(Guid id);                            
        Task UpdateRoleAsync(Guid userId, Guid roleId);
        Task UpdateAvatarAsync(Guid id, string avatarUrl);
        Task<PagedResponse<Models.User?>> GetAllPagedAsync(PagedRequest request);
    }
}

using DigitalProject.Data;
using DigitalProject.Interface.Role;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories.Role
{
    public class RoleRepository :IRoleRepository
    {
        private readonly DigitalVaultStoreDbContext _context;

        public RoleRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        public async Task<List<Models.Role>> GetAllAsync() =>
            await _context.Roles.ToListAsync();


        public async Task<Models.Role?> GetByCodeAsync(string code) =>
           await _context.Roles
               .FirstOrDefaultAsync(r => r.Code == code);

        public async Task<Models.Role?> GetByIdAsync(Guid id) =>
             await _context.Roles.FindAsync(id);
    }
}

namespace DigitalProject.Interface.Role
{
    public interface IRoleRepository
    {
        Task<Models.Role> GetByCodeAsync(string code);
        Task<List<Models.Role>> GetAllAsync();
        Task<Models.Role?> GetByIdAsync(Guid id);
    }
}

using DigitalProject.Models;
using DigitalProject.Request;
using DigitalProject.Response;

namespace DigitalProject.Interface.Payment
{
    public interface IPaymentRepository
    {
        Task<Models.Payment> GetByIdAsync(Guid id);
        Task<List<Models.Payment>> GetByOrderIdAsync(Guid orderId);
        Task<Models.Payment?> GetActiveByOrderIdAsync(Guid orderId);
        Task<bool> CreateAsync(Models.Payment payment);
        Task<bool> UpdateAsync(Models.Payment payment);
        Task<List<Models.Payment>> GetAllAsync();
        Task<PagedResponse<Models.Payment>> GetAllPagedAsync(PagedRequest request);
    }
}

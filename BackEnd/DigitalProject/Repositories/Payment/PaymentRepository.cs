using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Interface.Payment;
using DigitalProject.Request;
using DigitalProject.Response;
using Microsoft.EntityFrameworkCore;

namespace DigitalProject.Repositories.Payment
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DigitalVaultStoreDbContext _context;

        public PaymentRepository(DigitalVaultStoreDbContext context)
        {
            _context = context;
        }

        public async Task<Models.Payment> GetByIdAsync(Guid id)=>
        
           await _context.Payments
                .Include(p=>p.Order)
                 .FirstOrDefaultAsync(p => p.Id == id);



        public async Task<List<Models.Payment>> GetByOrderIdAsync(Guid orderId) =>
              await _context.Payments
                  .Include(p => p.Order)
                  .Where(p => p.OrderId == orderId)
                  .OrderByDescending(p => p.PaidAt)
                  .ToListAsync();
        public async Task<Models.Payment> GetActiveByOrderIdAsync(Guid orderId) =>
          await _context.Payments
              .FirstOrDefaultAsync(p =>
                  p.OrderId == orderId &&
                  p.IsVoid == false &&
                  p.Status == PaymentStatus.Paid);

        public async Task<bool> CreateAsync(Models.Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Models.Payment payment)
        {
            _context.Payments.Update(payment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Models.Payment>> GetAllAsync() =>
        await _context.Payments
            .Include(p => p.Order)
             .ThenInclude(o => o.User)
            .Include(p => p.VoidByUser)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        public async Task<PagedResponse<Models.Payment>> GetAllPagedAsync(PagedRequest request)
        {
            var queryable = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.User)
                .Include(p => p.VoidByUser)
                .OrderByDescending(p => p.PaidAt)
                .AsQueryable();

            var total = await queryable.CountAsync();

            var data = await queryable
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<Models.Payment>
            {
                Data = data,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}

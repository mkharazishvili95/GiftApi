using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        readonly ApplicationDbContext _db;
        public VoucherRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Voucher?> Create(Voucher? voucher)
        {
            _db.Vouchers.Add(voucher);

            return voucher;
        }

        public async Task<Voucher?> Edit(Voucher? voucher)
        {
            _db.Vouchers.Update(voucher);
            return voucher;
        }

        public async Task<Voucher?> GetByIdAsync(Guid id)
        {
            return await _db.Vouchers.FindAsync(id);
        }

        public Task<Voucher?> GetWithCategoryAndBrand(Guid id)
        {
            return _db.Vouchers
                .Include(x => x.Brand)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}

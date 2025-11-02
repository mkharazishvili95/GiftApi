using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;

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
    }
}

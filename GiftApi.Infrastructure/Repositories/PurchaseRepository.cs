using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        readonly ApplicationDbContext _db;
        public PurchaseRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<VoucherDeliveryInfo?> Get(Guid id)
        {
            return await _db.VoucherDeliveryInfos.FindAsync(id);
        }

        public async Task<List<VoucherDeliveryInfo>?> GetAll()
        {
            return await _db.VoucherDeliveryInfos.AsNoTracking().Include(x => x.Voucher).ToListAsync();
        }
    }
}

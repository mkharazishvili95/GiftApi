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
            return await _db.VoucherDeliveryInfos
                .AsNoTracking()
                .Include(x => x.Voucher)
                .ToListAsync();
        }

        public async Task<bool> RedeemAsync(Guid deliveryInfoId, Guid performedByUserId)
        {
            var entity = await _db.VoucherDeliveryInfos
                .Include(x => x.Voucher)
                .FirstOrDefaultAsync(x => x.Id == deliveryInfoId);

            if (entity == null) return false;
            if (entity.IsUsed == true) return false;

            if (entity.Voucher != null)
            {
                var expiry = entity.Voucher.CreateDate.AddMonths(entity.Voucher.ValidMonths);
                if (DateTime.UtcNow > expiry && !entity.Voucher.Unlimited)
                    return false;
            }

            var qty = entity.Quantity ?? 1;

            var prevUsed = entity.IsUsed ?? false;

            entity.IsUsed = true;
            entity.UsedDate = DateTime.UtcNow;

            if (entity.Voucher != null)
            {
                entity.Voucher.Redeemed += qty;
            }

            _db.VoucherRedeemAudits.Add(new VoucherRedeemAudit
            {
                Id = Guid.NewGuid(),
                DeliveryInfoId = entity.Id,
                VoucherId = entity.VoucherId,
                PerformedByUserId = performedByUserId,
                Action = "Redeem",
                PerformedAt = DateTime.UtcNow,
                Quantity = qty,
                PreviousIsUsed = prevUsed,
                NewIsUsed = true
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UndoRedeemAsync(Guid deliveryInfoId, Guid performedByUserId)
        {
            var entity = await _db.VoucherDeliveryInfos
                .Include(x => x.Voucher)
                .FirstOrDefaultAsync(x => x.Id == deliveryInfoId);

            if (entity == null) return false;
            if (entity.IsUsed != true) return false;

            var qty = entity.Quantity ?? 1;
            var prevUsed = entity.IsUsed ?? false;

            entity.IsUsed = false;
            entity.UsedDate = null;

            if (entity.Voucher != null)
            {
                entity.Voucher.Redeemed -= qty;
                if (entity.Voucher.Redeemed < 0)
                    entity.Voucher.Redeemed = 0;
            }

            _db.VoucherRedeemAudits.Add(new VoucherRedeemAudit
            {
                Id = Guid.NewGuid(),
                DeliveryInfoId = entity.Id,
                VoucherId = entity.VoucherId,
                PerformedByUserId = performedByUserId,
                Action = "UndoRedeem",
                PerformedAt = DateTime.UtcNow,
                Quantity = qty,
                PreviousIsUsed = prevUsed,
                NewIsUsed = false
            });

            await _db.SaveChangesAsync();
            return true;
        }
    }
}

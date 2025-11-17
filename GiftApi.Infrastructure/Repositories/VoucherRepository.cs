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

        public Task<Voucher?> Create(Voucher? voucher)
        {
            _db.Vouchers.Add(voucher);
            return Task.FromResult(voucher);
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

        public async Task<Voucher?> GetWithCategoryAndBrand(Guid id)
        {
            return await _db.Vouchers
                .Include(x => x.Brand)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Voucher>?> GetAllWithCategoryAndBrand()
        {
            var vouchers = await _db.Vouchers
                .Include(v => v.Brand)
                .ThenInclude(b => b.Category)
                .Where(v => v.IsActive && !v.IsDeleted)
                .ToListAsync();

            foreach (var v in vouchers)
            {
                if (v.Brand != null && v.Brand.IsDeleted)
                {
                    v.Brand = null;
                }
                else if (v.Brand?.Category != null && v.Brand.Category.IsDeleted)
                {
                    v.Brand.Category = null;
                }
            }

            return vouchers;
        }

        public async Task<bool> Buy(
            Guid voucherId,
            Guid userId,
            int quantity,
            string recipientName,
            string recipientPhone,
            string recipientCity,
            string recipientAddress,
            string? recipientEmail = null,
            string? message = null,
            string? senderName = null,
            Guid? senderId = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return false;

            var voucher = await _db.Vouchers
                .FirstOrDefaultAsync(x => x.Id == voucherId && x.IsActive && !x.IsDeleted);

            if (voucher == null || voucher.Quantity < quantity)
                return false;

            voucher.Quantity -= quantity;
            voucher.SoldCount = (voucher.SoldCount ?? 0) + quantity;
            _db.Vouchers.Update(voucher);

            var deliveryInfo = new VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                SenderName = senderName,
                RecipientName = recipientName,
                RecipientEmail = recipientEmail,
                RecipientPhone = recipientPhone,
                RecipientCity = recipientCity,
                RecipientAddress = recipientAddress,
                Message = message,
                SenderId = user.Id,
                Quantity = quantity,
                IsUsed = false
            };

            _db.VoucherDeliveryInfos.Add(deliveryInfo);

            return true;
        }

        public async Task<VoucherDeliveryInfo?> GetDeliveryInfoByIdAsync(Guid id)
        {
            return await _db.VoucherDeliveryInfos.FindAsync(id);
        }

        public async Task<List<VoucherDeliveryInfo>> GetDeliveryInfosBySenderAsync(Guid senderId, bool includeVoucher, bool? isUsedFilter, int page, int pageSize)
        {
            var query = _db.VoucherDeliveryInfos
                .AsQueryable()
                .Where(x => x.SenderId == senderId);

            if (isUsedFilter.HasValue)
                query = query.Where(x => x.IsUsed == isUsedFilter.Value);

            if (includeVoucher)
            {
                query = query
                    .Include(x => x.Voucher)
                    .ThenInclude(v => v.Brand)
                    .ThenInclude(b => b.Category);
            }

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> Delete(Guid id)
        {
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(x => x.Id == id);
            if (voucher == null || voucher.IsDeleted) return false;

            voucher.IsDeleted = true;
            voucher.IsActive = false;
            voucher.UpdateDate = DateTime.UtcNow.AddHours(4);
            voucher.DeleteDate = DateTime.UtcNow.AddHours(4);
            _db.Vouchers.Update(voucher);
            return true;
        }

        public async Task<bool> Restore(Guid id)
        {
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(x => x.Id == id);
            if (voucher == null || !voucher.IsDeleted) return false;

            voucher.IsDeleted = false;
            voucher.DeleteDate = null;
            voucher.UpdateDate = DateTime.UtcNow.AddHours(4);
            _db.Vouchers.Update(voucher);
            return true;
        }

        public async Task<List<Voucher>> SearchAsync(int? brandId, int? categoryId, decimal? minAmount, decimal? maxAmount, string? term)
        {
            var query = _db.Vouchers
                .Include(v => v.Brand)
                .ThenInclude(b => b.Category)
                .Where(v => v.IsActive && !v.IsDeleted)
                .AsQueryable();

            if (brandId.HasValue)
                query = query.Where(v => v.BrandId == brandId.Value);

            if (categoryId.HasValue)
                query = query.Where(v => v.Brand != null && v.Brand.CategoryId == categoryId.Value);

            if (minAmount.HasValue)
                query = query.Where(v => v.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(v => v.Amount <= maxAmount.Value);

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim().ToLower();
                query = query.Where(v =>
                    v.Title.ToLower().Contains(t) ||
                    v.Description.ToLower().Contains(t) ||
                    (v.Brand != null && v.Brand.Name.ToLower().Contains(t)) ||
                    (v.Brand != null && v.Brand.Category != null && v.Brand.Category.Name.ToLower().Contains(t))
                );
            }
            var list = await query.ToListAsync();

            int ExtractTrailingNumber(string title)
            {
                if (string.IsNullOrWhiteSpace(title)) return int.MaxValue;
                var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var last = parts.Last();
                return int.TryParse(last, out var n) ? n : int.MaxValue;
            }

            list = list
                .OrderBy(v => ExtractTrailingNumber(v.Title))
                .ThenBy(v => v.Title)
                .ToList();

            foreach (var v in list)
            {
                if (v.Brand != null && v.Brand.IsDeleted)
                {
                    v.Brand = null;
                }
                else if (v.Brand?.Category != null && v.Brand.Category.IsDeleted)
                {
                    v.Brand.Category = null;
                }
            }

            return list;
        }

        //Bulk-ისთვის:
        public async Task<List<Voucher>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return new List<Voucher>();
            return await _db.Vouchers.Where(v => idList.Contains(v.Id)).ToListAsync();
        }

        public Task AddRangeAsync(IEnumerable<Voucher> vouchers)
        {
            _db.Vouchers.AddRange(vouchers);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Voucher> vouchers)
        {
            _db.Vouchers.UpdateRange(vouchers);
            return Task.CompletedTask;
        }
    }
}

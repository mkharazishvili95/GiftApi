using GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class VoucherStatisticsRepository : IVoucherStatisticsRepository
    {
        private readonly ApplicationDbContext _db;
        public VoucherStatisticsRepository(ApplicationDbContext db) => _db = db;

        public async Task<VoucherUsageStatsResponse> GetVoucherUsageStatsAsync(
            int? brandId,
            string? search,
            bool includeInactive,
            string? orderBy,
            bool desc,
            CancellationToken cancellationToken)
        {
            var vouchers = _db.Vouchers
                .Include(v => v.Brand)
                .Where(v => !v.IsDeleted);

            if (!includeInactive)
                vouchers = vouchers.Where(v => v.IsActive);

            if (brandId.HasValue)
                vouchers = vouchers.Where(v => v.BrandId == brandId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                vouchers = vouchers.Where(v =>
                    v.Title.ToLower().Contains(s) ||
                    (v.Brand != null && v.Brand.Name.ToLower().Contains(s)));
            }

            var projected = vouchers.Select(v => new VoucherUsageStatsItem
            {
                VoucherId = v.Id,
                Title = v.Title,
                BrandName = v.Brand != null ? v.Brand.Name : null,
                Unlimited = v.Unlimited,
                IsActive = v.IsActive,
                Sold = v.SoldCount ?? 0,
                Redeemed = v.Redeemed,
                Remaining = v.Unlimited ? (v.SoldCount ?? 0) - v.Redeemed : (v.Quantity - v.Redeemed),
                UsageRate = (v.SoldCount ?? 0) > 0 ? (decimal)v.Redeemed / (decimal)(v.SoldCount ?? 0) : 0m,
                CreateDate = v.CreateDate,
                ExpiryDate = v.Unlimited ? null : v.CreateDate.AddMonths(v.ValidMonths)
            });

            projected = orderBy?.ToLower() switch
            {
                "sold" => desc ? projected.OrderByDescending(x => x.Sold) : projected.OrderBy(x => x.Sold),
                "redeemed" => desc ? projected.OrderByDescending(x => x.Redeemed) : projected.OrderBy(x => x.Redeemed),
                "usage" => desc ? projected.OrderByDescending(x => x.UsageRate) : projected.OrderBy(x => x.UsageRate),
                "remaining" => desc ? projected.OrderByDescending(x => x.Remaining) : projected.OrderBy(x => x.Remaining),
                _ => projected.OrderByDescending(x => x.CreateDate)
            };

            var list = await projected.ToListAsync(cancellationToken);

            var response = new VoucherUsageStatsResponse
            {
                Items = list,
                TotalVouchers = await _db.Vouchers.CountAsync(cancellationToken),
                TotalActiveVouchers = await _db.Vouchers.CountAsync(v => v.IsActive && !v.IsDeleted, cancellationToken),
                TotalSold = list.Sum(x => x.Sold),
                TotalRedeemed = list.Sum(x => x.Redeemed),
                TotalRemaining = list.Sum(x => x.Remaining < 0 ? 0 : x.Remaining),
                AverageUsageRate = list.Count == 0 ? 0 :
                    Math.Round(list.Average(x => x.UsageRate), 4)
            };

            return response;
        }
    }
}
using GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats;
using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class VoucherStatisticsRepository : IVoucherStatisticsRepository
    {
        readonly ApplicationDbContext _db;
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

        public async Task<VoucherStatisticsResponse> GetVoucherStatisticsAsync(
            int? brandId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int lowStockThreshold,
            int expiringInDays,
            bool includeInactive,
            int topSoldTake,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var expiryLimit = now.AddDays(expiringInDays);

            var vouchers = _db.Vouchers
                .Include(v => v.Brand)
                .Where(v => !v.IsDeleted);

            if (!includeInactive)
                vouchers = vouchers.Where(v => v.IsActive);

            if (brandId.HasValue)
                vouchers = vouchers.Where(v => v.BrandId == brandId.Value);

            if (fromUtc.HasValue)
                vouchers = vouchers.Where(v => v.CreateDate >= fromUtc.Value);
            if (toUtc.HasValue)
                vouchers = vouchers.Where(v => v.CreateDate <= toUtc.Value);

            var baseProjection = vouchers.Select(v => new VoucherStatisticsItemsResponse
            {
                VoucherId = v.Id,
                Title = v.Title,
                BrandName = v.Brand != null ? v.Brand.Name : null,
                Unlimited = v.Unlimited,
                IsActive = v.IsActive,
                Quantity = v.Quantity,
                Redeemed = v.Redeemed,
                SoldCount = v.SoldCount ?? 0,
                Remaining = v.Unlimited
                    ? (v.SoldCount ?? 0) - v.Redeemed
                    : (v.Quantity - v.Redeemed),
                CreateDate = v.CreateDate,
                ExpiryDate = v.Unlimited ? null : v.CreateDate.AddMonths(v.ValidMonths)
            });

            var topSold = await baseProjection
                .OrderByDescending(x => x.SoldCount)
                .ThenBy(x => x.Title)
                .Take(topSoldTake)
                .ToListAsync(cancellationToken);

            var lowStock = await baseProjection
                .Where(x => !x.Unlimited && x.Remaining < lowStockThreshold)
                .OrderBy(x => x.Remaining)
                .ThenByDescending(x => x.SoldCount)
                .ToListAsync(cancellationToken);

            var expiringSoon = await baseProjection
                .Where(x =>
                    !x.Unlimited &&
                    x.ExpiryDate != null &&
                    x.ExpiryDate >= now &&
                    x.ExpiryDate <= expiryLimit)
                .OrderBy(x => x.ExpiryDate)
                .ToListAsync(cancellationToken);

            return new VoucherStatisticsResponse
            {
                TopSold = topSold,
                LowStock = lowStock,
                ExpiringSoon = expiringSoon,
                TotalVouchersConsidered = await vouchers.CountAsync(cancellationToken)
            };
        }
        public async Task<List<VoucherStatisticsItemsResponse>> GetExpiringSoonAsync(
            int? brandId,
            int days,
            bool includeInactive,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var limit = now.AddDays(days);

            var query = _db.Vouchers
                .Include(v => v.Brand)
                .Where(v => !v.IsDeleted);

            if (!includeInactive)
                query = query.Where(v => v.IsActive);

            if (brandId.HasValue)
                query = query.Where(v => v.BrandId == brandId.Value);

            return await query
                .Where(v => !v.Unlimited)
                .Select(v => new VoucherStatisticsItemsResponse
                {
                    VoucherId = v.Id,
                    Title = v.Title,
                    BrandName = v.Brand != null ? v.Brand.Name : null,
                    Unlimited = v.Unlimited,
                    IsActive = v.IsActive,
                    Quantity = v.Quantity,
                    Redeemed = v.Redeemed,
                    SoldCount = v.SoldCount ?? 0,
                    Remaining = v.Unlimited
                        ? (v.SoldCount ?? 0) - v.Redeemed
                        : (v.Quantity - v.Redeemed),
                    CreateDate = v.CreateDate,
                    ExpiryDate = v.Unlimited ? null : v.CreateDate.AddMonths(v.ValidMonths)
                })
                .Where(x =>
                    x.ExpiryDate != null &&
                    x.ExpiryDate >= now &&
                    x.ExpiryDate <= limit)
                .OrderBy(x => x.ExpiryDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<DailyUsageRow>> GetDailyUsageAsync(int? brandId, int days, CancellationToken ct)
        {
            if (days <= 0) days = 1;

            var todayUtc = DateTime.UtcNow.Date;
            var sinceInclusive = todayUtc.AddDays(-days + 1);

            var vouchersQuery = _db.Vouchers
                .Where(v => !v.IsDeleted);

            if (brandId.HasValue)
                vouchersQuery = vouchersQuery.Where(v => v.BrandId == brandId.Value);

            var soldGrouped = await _db.VoucherDeliveryInfos
                .Where(d => d.CreateDate != null && d.CreateDate!.Value.Date >= sinceInclusive)
                .Join(vouchersQuery,
                    d => d.VoucherId,
                    v => v.Id,
                    (d, v) => new { Day = d.CreateDate!.Value.Date, Qty = d.Quantity ?? 1 })
                .GroupBy(x => x.Day)
                .Select(g => new { Day = g.Key, Sold = g.Sum(x => x.Qty) })
                .ToListAsync(ct);

            var redeemedGrouped = await _db.VoucherRedeemAudits
                .Where(r => r.PerformedAt.Date >= sinceInclusive)
                .Join(vouchersQuery,
                    r => r.VoucherId,
                    v => v.Id,
                    (r, v) => new { Day = r.PerformedAt.Date, Qty = r.Quantity })
                .GroupBy(x => x.Day)
                .Select(g => new { Day = g.Key, Redeemed = g.Sum(x => x.Qty) })
                .ToListAsync(ct);

            var allDays = Enumerable.Range(0, days)
                .Select(i => sinceInclusive.AddDays(i))
                .ToList();

            var rows = allDays.Select(day =>
            {
                var sold = soldGrouped.FirstOrDefault(x => x.Day == day)?.Sold ?? 0;
                var redeemed = redeemedGrouped.FirstOrDefault(x => x.Day == day)?.Redeemed ?? 0;
                return new DailyUsageRow
                {
                    Day = day,
                    Sold = sold,
                    Redeemed = redeemed
                };
            }).ToList();

            return rows;
        }

        public async Task<List<BrandRedemptionLeaderboardItem>> GetBrandRedemptionLeaderboardAsync(int top, CancellationToken ct)
        {
            if (top <= 0) top = 10;

            var data = await _db.Vouchers
                .Include(v => v.Brand)
                .Where(v => !v.IsDeleted && v.Brand != null)
                .GroupBy(v => new { v.BrandId, v.Brand!.Name })
                .Select(g => new BrandRedemptionLeaderboardItem
                {
                    BrandId = g.Key.BrandId.GetValueOrDefault(),
                    BrandName = g.Key.Name,
                    Sold = g.Sum(v => v.SoldCount ?? 0),
                    Redeemed = g.Sum(v => v.Redeemed),
                    Remaining = g.Sum(v => v.Unlimited
                        ? Math.Max((v.SoldCount ?? 0) - v.Redeemed, 0)
                        : Math.Max(v.Quantity - v.Redeemed, 0))
                })
                .ToListAsync(ct);

            var ordered = data
                .OrderByDescending(x => x.RedemptionRate)
                .ThenByDescending(x => x.Redeemed)
                .ThenBy(x => x.BrandName)
                .Take(top)
                .ToList();

            return ordered;
        }
    }
}
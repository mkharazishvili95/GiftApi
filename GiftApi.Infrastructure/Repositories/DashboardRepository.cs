using GiftApi.Application.Features.Manage.Dashboard.Queries.GetGlobalMetrics;
using GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        readonly ApplicationDbContext _db;
        public DashboardRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardSummaryResponse> GetSummaryAsync(int expiringInDays, int topBrands, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var expiryLimit = now.AddDays(expiringInDays);

            var vouchersQuery = _db.Vouchers.Include(v => v.Brand).Where(v => !v.IsDeleted);

            var usersTotalTask = _db.Users.CountAsync(cancellationToken);
            var activeVouchersTask = vouchersQuery.CountAsync(v => v.IsActive, cancellationToken);
            var soldTotalTask = vouchersQuery.SumAsync(v => (int?)v.SoldCount, cancellationToken);
            var redeemedTotalTask = vouchersQuery.SumAsync(v => (int?)v.Redeemed, cancellationToken);

            var unusedTotalTask = vouchersQuery.SumAsync(v =>
                (int?)(
                    v.Unlimited
                        ? Math.Max((v.SoldCount ?? 0) - v.Redeemed, 0)
                        : Math.Max(v.Quantity - v.Redeemed, 0)
                ), cancellationToken);

            var revenueTask = vouchersQuery
                .Where(v => !v.IsPercentage)
                .SumAsync(v => (decimal?)(v.Redeemed * v.Amount), cancellationToken);

            var purchasesTotalTask = _db.VoucherDeliveryInfos.CountAsync(cancellationToken);
            var usedPurchasesTask = _db.VoucherDeliveryInfos.CountAsync(x => x.IsUsed == true, cancellationToken);

            var expiringSoonTask = vouchersQuery.CountAsync(v =>
                !v.Unlimited &&
                v.IsActive &&
                v.CreateDate.AddMonths(v.ValidMonths) >= now &&
                v.CreateDate.AddMonths(v.ValidMonths) <= expiryLimit,
                cancellationToken);

            await Task.WhenAll(
                usersTotalTask,
                activeVouchersTask,
                soldTotalTask,
                redeemedTotalTask,
                unusedTotalTask,
                revenueTask,
                purchasesTotalTask,
                usedPurchasesTask,
                expiringSoonTask
            );

            var toppBrands = await vouchersQuery
                .Where(v => v.Brand != null)
                .GroupBy(v => v.Brand.Name)
                .Select(g => new
                {
                    BrandName = g.Key,
                    VouchersCount = g.Count(),
                    TotalSoldCount = g.Sum(x => x.SoldCount ?? 0)
                })
                .OrderByDescending(x => x.TotalSoldCount)
                .ThenByDescending(x => x.VouchersCount)
                .Take(topBrands)
                .ToListAsync(cancellationToken);

            return new DashboardSummaryResponse
            {
                UsersTotal = usersTotalTask.Result,
                ActiveVouchers = activeVouchersTask.Result,
                SoldVouchersTotal = soldTotalTask.Result ?? 0,
                RedeemedTotal = redeemedTotalTask.Result ?? 0,
                UnusedVouchersTotal = unusedTotalTask.Result ?? 0,
                TotalRevenueEstimate = revenueTask.Result ?? 0,
                PurchasesTotal = purchasesTotalTask.Result,
                UsedPurchases = usedPurchasesTask.Result,
                UnusedPurchases = purchasesTotalTask.Result - usedPurchasesTask.Result,
                ExpiringSoonCount = expiringSoonTask.Result,
                TopBrands = toppBrands.Select(x => new TopBrandItem
                {
                    BrandName = x.BrandName,
                    VouchersCount = x.VouchersCount,
                    TotalSoldCount = x.TotalSoldCount
                }).ToList()
            };
        }

        public async Task<GetGlobalMetricsResponse> GetGlobalMetricsAsync(CancellationToken cancellationToken)
        {
            var todayStart = DateTime.UtcNow.Date.AddHours(4);
            var todayEnd = todayStart.AddDays(1);

            var soldToday = await _db.VoucherDeliveryInfos
                .CountAsync(x => x.CreateDate >= todayStart && x.CreateDate < todayEnd, cancellationToken);

            var revenueToday = await _db.VoucherDeliveryInfos
                .Include(x => x.Voucher)
                .Where(x => x.CreateDate >= todayStart && x.CreateDate < todayEnd && x.Voucher != null)
                .SumAsync(x => x.Voucher.Amount * (x.Quantity ?? 1), cancellationToken);

            var usersCountTask = _db.Users.CountAsync(cancellationToken);
            var activeVouchersTask = _db.Vouchers.CountAsync(v => v.IsActive && !v.IsDeleted, cancellationToken);

            await Task.WhenAll(usersCountTask, activeVouchersTask);

            return new GetGlobalMetricsResponse
            {
                UsersCount = usersCountTask.Result,
                ActiveVouchers = activeVouchersTask.Result,
                SoldToday = soldToday,
                RevenueToday = revenueToday
            };
        }
    }
}
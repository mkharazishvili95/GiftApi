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

            var vouchersQuery = _db.Vouchers.Where(v => !v.IsDeleted);

            var usersTotalTask = _db.Users.CountAsync(cancellationToken);
            var activeVouchersTask = vouchersQuery.CountAsync(cancellationToken);
            var soldTotalTask = vouchersQuery.SumAsync(v => (int?)(v.SoldCount ?? 0), cancellationToken);
            var redeemedTotalTask = vouchersQuery.SumAsync(v => (int?)v.Redeemed, cancellationToken);
            var unusedTotalTask = vouchersQuery.SumAsync(v => (int?)(v.Quantity - v.Redeemed), cancellationToken);
            var revenueTask = vouchersQuery
                .Where(v => !v.IsPercentage)
                .SumAsync(v => v.Redeemed * v.Amount, cancellationToken);
            var purchasesTotalTask = _db.VoucherDeliveryInfos.CountAsync(cancellationToken);
            var usedPurchasesTask = _db.VoucherDeliveryInfos.CountAsync(x => x.IsUsed == true, cancellationToken);
            var expiringSoonTask = vouchersQuery.CountAsync(v =>
                !v.Unlimited &&
                v.CreateDate.AddMonths(v.ValidMonths) <= expiryLimit &&
                v.CreateDate.AddMonths(v.ValidMonths) >= now, cancellationToken);

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
                TotalRevenueEstimate = revenueTask.Result,
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
    }
}
using GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats;
using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;

namespace GiftApi.Application.Interfaces
{
    public interface IVoucherStatisticsRepository
    {
        Task<VoucherUsageStatsResponse> GetVoucherUsageStatsAsync(
            int? brandId,
            string? search,
            bool includeInactive,
            string? orderBy,
            bool desc,
            CancellationToken cancellationToken);

        Task<VoucherStatisticsResponse> GetVoucherStatisticsAsync(
            int? brandId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int lowStockThreshold,
            int expiringInDays,
            bool includeInactive,
            int topSoldTake,
            CancellationToken cancellationToken);

        Task<List<VoucherStatisticsItemsResponse>> GetExpiringSoonAsync(
            int? brandId,
            int days,
            bool includeInactive,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<DailyUsageRow>> GetDailyUsageAsync(int? brandId, int days, CancellationToken ct);
        Task<List<BrandRedemptionLeaderboardItem>> GetBrandRedemptionLeaderboardAsync(int top, CancellationToken ct);
    }

    public sealed class DailyUsageRow
    {
        public DateTime Day { get; set; }
        public int Sold { get; set; }
        public int Redeemed { get; set; }
    }
}
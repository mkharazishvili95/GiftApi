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
    }
}
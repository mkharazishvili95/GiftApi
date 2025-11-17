using GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats;

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
    }
}
using GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary;
using GiftApi.Application.Features.Manage.Dashboard.Queries.GetGlobalMetrics;

namespace GiftApi.Application.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryResponse> GetSummaryAsync(int expiringInDays, int topBrands, CancellationToken cancellationToken);
        Task<GetGlobalMetricsResponse> GetGlobalMetricsAsync(CancellationToken cancellationToken); 
    }
}
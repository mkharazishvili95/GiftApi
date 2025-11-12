using GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary;

namespace GiftApi.Application.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryResponse> GetSummaryAsync(int expiringInDays, int topBrands, CancellationToken cancellationToken);
    }
}
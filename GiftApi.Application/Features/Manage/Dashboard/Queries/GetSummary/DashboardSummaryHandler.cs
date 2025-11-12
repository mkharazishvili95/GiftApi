using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary
{
    public class DashboardSummaryHandler : IRequestHandler<DashboardSummaryQuery, DashboardSummaryResponse>
    {
        readonly IDashboardRepository _dashboardRepository;
        public DashboardSummaryHandler(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardSummaryResponse> Handle(DashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            return await _dashboardRepository.GetSummaryAsync(request.ExpiringInDays, request.TopBrands, cancellationToken);
        }
    }
}
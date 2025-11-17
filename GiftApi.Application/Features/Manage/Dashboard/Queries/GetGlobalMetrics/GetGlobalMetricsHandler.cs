using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Dashboard.Queries.GetGlobalMetrics
{
    public class GetGlobalMetricsHandler : IRequestHandler<GetGlobalMetricsQuery, GetGlobalMetricsResponse>
    {
        readonly IDashboardRepository _dashboardRepository;
        public GetGlobalMetricsHandler(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public Task<GetGlobalMetricsResponse> Handle(GetGlobalMetricsQuery request, CancellationToken cancellationToken)
            => _dashboardRepository.GetGlobalMetricsAsync(cancellationToken);
    }
}
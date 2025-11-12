using MediatR;

namespace GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary
{
    public class DashboardSummaryQuery : IRequest<DashboardSummaryResponse>
    {
        public int ExpiringInDays { get; set; } = 30;
        public int TopBrands { get; set; } = 5;
    }
}
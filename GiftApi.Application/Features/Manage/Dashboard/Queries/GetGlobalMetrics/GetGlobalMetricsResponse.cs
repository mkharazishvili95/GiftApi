namespace GiftApi.Application.Features.Manage.Dashboard.Queries.GetGlobalMetrics
{
    public class GetGlobalMetricsResponse
    {
        public int UsersCount { get; set; }
        public int ActiveVouchers { get; set; }
        public int SoldToday { get; set; }
        public decimal RevenueToday { get; set; }
    }
}
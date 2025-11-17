using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public class VoucherStatisticsQuery : IRequest<VoucherStatisticsResponse>
    {
        public int? BrandId { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        public int ExpiringInDays { get; set; } = 30;
        public bool IncludeInactive { get; set; } = false;
        public int TopSoldTake { get; set; } = 10;
    }
}
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public sealed record VoucherUsageTrendQuery(int? BrandId, int Days = 30)
        : IRequest<VoucherUsageTrendResponse>;

    public sealed class VoucherUsageTrendResponse
    {
        public List<VoucherUsageTrendPoint> Points { get; set; } = new();
        public int TotalSold { get; set; }
        public int TotalRedeemed { get; set; }
        public decimal RedemptionRate => TotalSold == 0 ? 0 : (decimal)TotalRedeemed / TotalSold;
    }

    public sealed class VoucherUsageTrendPoint
    {
        public DateOnly Day { get; set; }
        public int Sold { get; set; }
        public int Redeemed { get; set; }
    }
}
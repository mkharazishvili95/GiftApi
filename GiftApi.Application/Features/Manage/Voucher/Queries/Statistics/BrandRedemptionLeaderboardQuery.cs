using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public sealed record BrandRedemptionLeaderboardQuery(int Top = 10)
        : IRequest<BrandRedemptionLeaderboardResponse>;

    public sealed class BrandRedemptionLeaderboardResponse
    {
        public List<BrandRedemptionLeaderboardItem> Items { get; set; } = new();
        public int TotalBrands { get; set; }
    }

    public sealed class BrandRedemptionLeaderboardItem
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public int Sold { get; set; }
        public int Redeemed { get; set; }
        public int Remaining { get; set; }
        public decimal RedemptionRate => Sold == 0 ? 0 : (decimal)Redeemed / Sold;
    }
}
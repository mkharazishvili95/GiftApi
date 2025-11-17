using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats
{
    public class VoucherUsageStatsQuery : IRequest<VoucherUsageStatsResponse>
    {
        public int? BrandId { get; set; }
        public string? Search { get; set; }
        public bool IncludeInactive { get; set; } = false;
        public string? OrderBy { get; set; }
        public bool Desc { get; set; } = true;
    }

    public class VoucherUsageStatsResponse
    {
        public List<VoucherUsageStatsItem> Items { get; set; } = new();
        public int TotalVouchers { get; set; }
        public int TotalActiveVouchers { get; set; }
        public int TotalSold { get; set; }
        public int TotalRedeemed { get; set; }
        public int TotalRemaining { get; set; }
        public decimal AverageUsageRate { get; set; }
    }

    public class VoucherUsageStatsItem
    {
        public Guid VoucherId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public bool Unlimited { get; set; }
        public bool IsActive { get; set; }
        public int Sold { get; set; }
        public int Redeemed { get; set; }
        public int Remaining { get; set; }       
        public decimal UsageRate { get; set; }   
        public DateTime CreateDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
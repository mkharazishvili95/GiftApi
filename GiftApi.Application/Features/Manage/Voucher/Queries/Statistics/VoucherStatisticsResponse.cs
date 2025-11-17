namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public class VoucherStatisticsResponse
    {
        public List<VoucherStatItem> TopSold { get; set; } = new();
        public List<VoucherStatItem> LowStock { get; set; } = new();
        public List<VoucherStatItem> ExpiringSoon { get; set; } = new();
        public int TotalVouchersConsidered { get; set; }
    }

    public class VoucherStatItem
    {
        public Guid VoucherId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public bool Unlimited { get; set; }
        public bool IsActive { get; set; }
        public int Quantity { get; set; }
        public int Redeemed { get; set; }
        public int SoldCount { get; set; }
        public int Remaining { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}

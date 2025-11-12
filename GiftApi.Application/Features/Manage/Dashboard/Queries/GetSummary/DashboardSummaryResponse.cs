namespace GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary
{
    public class DashboardSummaryResponse
    {
        public int UsersTotal { get; set; }
        public int ActiveVouchers { get; set; }
        public int SoldVouchersTotal { get; set; }
        public int RedeemedTotal { get; set; }
        public int UnusedVouchersTotal { get; set; }
        public decimal TotalRevenueEstimate { get; set; }
        public int PurchasesTotal { get; set; }
        public int UsedPurchases { get; set; }
        public int UnusedPurchases { get; set; }
        public int ExpiringSoonCount { get; set; }
        public List<TopBrandItem> TopBrands { get; set; } = new();
    }

    public class TopBrandItem
    {
        public string BrandName { get; set; } = string.Empty;
        public int VouchersCount { get; set; }
        public int TotalSoldCount { get; set; }
    }
}
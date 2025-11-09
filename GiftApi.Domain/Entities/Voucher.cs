namespace GiftApi.Domain.Entities
{
    public class Voucher
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
        public int? BrandId { get; set; }
        public Brand Brand { get; set; }
        public int ValidMonths { get; set; }
        public bool Unlimited { get; set; }
        public int Quantity { get; set; }
        public int Redeemed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
        public int? SoldCount { get; set; }
    }
}

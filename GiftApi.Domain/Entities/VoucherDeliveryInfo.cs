namespace GiftApi.Domain.Entities
{
    public class VoucherDeliveryInfo
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public Voucher Voucher { get; set; }
        public string? SenderName { get; set; }
        public string RecipientName { get; set; }
        public string? RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientAddress { get; set; }
        public string? Message { get; set; }
        public Guid? SenderId { get; set; }
        public int? Quantity { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
    }
}

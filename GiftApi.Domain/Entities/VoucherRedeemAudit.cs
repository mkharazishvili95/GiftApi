namespace GiftApi.Domain.Entities
{
    public class VoucherRedeemAudit
    {
        public Guid Id { get; set; }
        public Guid DeliveryInfoId { get; set; }
        public Guid VoucherId { get; set; }
        public Guid PerformedByUserId { get; set; }
        public string Action { get; set; } = default!;
        public DateTime PerformedAt { get; set; }
        public int Quantity { get; set; }
        public bool PreviousIsUsed { get; set; }
        public bool NewIsUsed { get; set; }
    }
}
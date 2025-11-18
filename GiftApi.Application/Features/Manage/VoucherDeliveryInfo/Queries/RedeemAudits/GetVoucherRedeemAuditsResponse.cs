using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.RedeemAudits
{
    public class GetVoucherRedeemAuditsResponse : BaseResponse
    {
        public Guid VoucherId { get; set; }
        public List<GetVoucherRedeemAuditItemResponse> Items { get; set; } = new();
    }

    public class GetVoucherRedeemAuditItemResponse
    {
        public Guid Id { get; set; }
        public Guid DeliveryInfoId { get; set; }
        public Guid PerformedByUserId { get; set; }
        public string Action { get; set; } = default!;
        public DateTime PerformedAt { get; set; }
        public int Quantity { get; set; }
        public bool PreviousIsUsed { get; set; }
        public bool NewIsUsed { get; set; }
    }
}
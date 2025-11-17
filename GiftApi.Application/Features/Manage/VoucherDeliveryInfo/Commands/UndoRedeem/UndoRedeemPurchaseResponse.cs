using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.UndoRedeem
{
    public class UndoRedeemPurchaseResponse : BaseResponse
    {
        public Guid DeliveryInfoId { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
    }
}
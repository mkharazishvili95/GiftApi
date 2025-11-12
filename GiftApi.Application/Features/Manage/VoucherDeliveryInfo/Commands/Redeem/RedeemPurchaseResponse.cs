using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.Redeem
{
    public class RedeemPurchaseResponse : BaseResponse
    {
        public Guid DeliveryInfoId { get; set; }
        public DateTime? UsedDate { get; set; }
    }
}
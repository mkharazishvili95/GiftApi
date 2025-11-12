using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.Redeem
{
    public class RedeemPurchaseCommand : IRequest<RedeemPurchaseResponse>
    {
        public Guid DeliveryInfoId { get; set; }
        public Guid PerformedByUserId { get; set; }
    }
}
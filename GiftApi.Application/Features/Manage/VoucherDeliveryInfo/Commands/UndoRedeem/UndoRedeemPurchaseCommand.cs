using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.UndoRedeem
{
    public class UndoRedeemPurchaseCommand : IRequest<UndoRedeemPurchaseResponse>
    {
        public Guid DeliveryInfoId { get; set; }
        public Guid PerformedByUserId { get; set; }
    }
}
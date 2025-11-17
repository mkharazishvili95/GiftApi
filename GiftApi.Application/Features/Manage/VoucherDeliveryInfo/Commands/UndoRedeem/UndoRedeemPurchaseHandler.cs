using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.UndoRedeem
{
    public class UndoRedeemPurchaseHandler : IRequestHandler<UndoRedeemPurchaseCommand, UndoRedeemPurchaseResponse>
    {
        readonly IPurchaseRepository _purchaseRepository;

        public UndoRedeemPurchaseHandler(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public async Task<UndoRedeemPurchaseResponse> Handle(UndoRedeemPurchaseCommand request, CancellationToken cancellationToken)
        {
            if (request.DeliveryInfoId == Guid.Empty)
                return new UndoRedeemPurchaseResponse { Success = false, StatusCode = 400, Message = "DeliveryInfoId is required." };

            var entity = await _purchaseRepository.Get(request.DeliveryInfoId);
            if (entity == null)
                return new UndoRedeemPurchaseResponse { Success = false, StatusCode = 404, Message = "Purchase not found." };

            if (entity.IsUsed != true)
                return new UndoRedeemPurchaseResponse { Success = false, StatusCode = 400, Message = "Purchase is not redeemed." };

            var ok = await _purchaseRepository.UndoRedeemAsync(request.DeliveryInfoId, request.PerformedByUserId);
            if (!ok)
                return new UndoRedeemPurchaseResponse { Success = false, StatusCode = 500, Message = "Undo redeem failed." };

            entity = await _purchaseRepository.Get(request.DeliveryInfoId);

            return new UndoRedeemPurchaseResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "Redeem undone successfully.",
                DeliveryInfoId = entity!.Id,
                IsUsed = entity.IsUsed ?? false,
                UsedDate = entity.UsedDate
            };
        }
    }
}
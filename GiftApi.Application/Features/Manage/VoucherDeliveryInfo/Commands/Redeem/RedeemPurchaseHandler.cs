using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.Redeem
{
    public class RedeemPurchaseHandler : IRequestHandler<RedeemPurchaseCommand, RedeemPurchaseResponse>
    {
        readonly IPurchaseRepository _purchaseRepository;

        public RedeemPurchaseHandler(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public async Task<RedeemPurchaseResponse> Handle(RedeemPurchaseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _purchaseRepository.Get(request.DeliveryInfoId);
            if (entity == null)
                return new RedeemPurchaseResponse { Success = false, Message = "Purchase not found" };

            if (entity.IsUsed == true)
                return new RedeemPurchaseResponse
                {
                    Success = false,
                    Message = "Already redeemed",
                    DeliveryInfoId = entity.Id,
                    UsedDate = entity.UsedDate
                };

            var ok = await _purchaseRepository.RedeemAsync(request.DeliveryInfoId, request.PerformedByUserId);
            if (!ok)
                return new RedeemPurchaseResponse { Success = false, Message = "Redeem failed (expired or invalid state)" };

            entity = await _purchaseRepository.Get(request.DeliveryInfoId);

            return new RedeemPurchaseResponse
            {
                StatusCode = 200,
                Success = true,
                Message = "Redeemed successfully",
                DeliveryInfoId = entity!.Id,
                UsedDate = entity.UsedDate
            };
        }
    }
}
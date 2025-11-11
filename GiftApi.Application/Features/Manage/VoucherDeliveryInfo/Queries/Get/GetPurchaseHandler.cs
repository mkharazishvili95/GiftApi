using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Get
{
    public class GetPurchaseHandler : IRequestHandler<GetPurchaseQuery, GetPurchaseResponse>
    {
        readonly IPurchaseRepository _purchaseRepository;
        public GetPurchaseHandler(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }
        public async Task<GetPurchaseResponse> Handle(GetPurchaseQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return new GetPurchaseResponse { Id = null, Success = false, Message = "Id is required.", StatusCode = 400 };

            var purchase = await _purchaseRepository.Get(request.Id);

            if (purchase == null)
                return new GetPurchaseResponse { Id = request.Id, Success = false, Message = $"Purchase with Id {request.Id} not found.", StatusCode = 404 };

            return new GetPurchaseResponse
            {
                Id = purchase.Id,
                VoucherId = purchase.VoucherId,
                SenderName = purchase.SenderName,
                RecipientName = purchase.RecipientName,
                RecipientEmail = purchase.RecipientEmail,
                RecipientPhone = purchase.RecipientPhone,
                RecipientCity = purchase.RecipientCity,
                RecipientAddress = purchase.RecipientAddress,
                Message = purchase.Message,
                SenderId = purchase.SenderId,
                Quantity = purchase.Quantity,
                IsUsed = purchase.IsUsed,
                SenderMessage = purchase.Message,
                Success = true,
                StatusCode = 200
            };
        }
    }
}

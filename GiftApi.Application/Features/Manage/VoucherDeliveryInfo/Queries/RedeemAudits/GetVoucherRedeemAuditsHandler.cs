using GiftApi.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.RedeemAudits
{
    public class GetVoucherRedeemAuditsHandler : IRequestHandler<GetVoucherRedeemAuditsQuery, GetVoucherRedeemAuditsResponse>
    {
        readonly IPurchaseRepository _purchaseRepository;
        public GetVoucherRedeemAuditsHandler(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public async Task<GetVoucherRedeemAuditsResponse> Handle(GetVoucherRedeemAuditsQuery request, CancellationToken cancellationToken)
        {
            if (request.VoucherId == Guid.Empty)
                return new GetVoucherRedeemAuditsResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "VoucherId is required."
                };

            var audits = await _purchaseRepository.GetVoucherRedeemAuditsAsync(request.VoucherId);

            var items = audits
                .OrderByDescending(a => a.PerformedAt)
                .Take(request.Take ?? 100)
                .Select(a => new GetVoucherRedeemAuditItemResponse
                {
                    Id = a.Id,
                    DeliveryInfoId = a.DeliveryInfoId,
                    PerformedByUserId = a.PerformedByUserId,
                    Action = a.Action,
                    PerformedAt = a.PerformedAt,
                    Quantity = a.Quantity,
                    PreviousIsUsed = a.PreviousIsUsed,
                    NewIsUsed = a.NewIsUsed
                })
                .ToList();

            return new GetVoucherRedeemAuditsResponse
            {
                Success = true,
                StatusCode = 200,
                VoucherId = request.VoucherId,
                Items = items
            };
        }
    }
}
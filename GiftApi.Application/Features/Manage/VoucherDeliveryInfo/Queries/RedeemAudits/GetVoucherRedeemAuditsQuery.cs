using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.RedeemAudits
{
    public class GetVoucherRedeemAuditsQuery : IRequest<GetVoucherRedeemAuditsResponse>
    {
        public Guid VoucherId { get; set; }
        public int? Take { get; set; } = 100;
    }
}
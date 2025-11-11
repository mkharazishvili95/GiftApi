using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Get
{
    public class GetPurchaseQuery : IRequest<GetPurchaseResponse>
    {
        public Guid Id { get; set; }
    }
}

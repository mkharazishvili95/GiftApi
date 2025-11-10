using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.ChangeStatus
{
    public class ChangeStatusCommand : IRequest<ChangeStatusResponse>
    {
        public Guid DeliveryInfoId { get; set; }
    }
}

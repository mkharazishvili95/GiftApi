using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Activate
{
    public class ActivateVoucherCommand : IRequest<ActivateVoucherResponse>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }
}
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Restore
{
    public class RestoreVoucherCommand : IRequest<RestoreVoucherResponse>
    {
        public Guid Id { get; set; }
    }
}
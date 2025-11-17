using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Delete
{
    public class DeleteVoucherCommand : IRequest<DeleteVoucherResponse>
    {
        public Guid Id { get; set; }
    }
}
using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Delete
{
    public class DeleteVoucherResponse : BaseResponse
    {
        public Guid Id { get; set; }
    }
}
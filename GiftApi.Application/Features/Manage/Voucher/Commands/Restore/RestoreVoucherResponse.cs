using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Restore
{
    public class RestoreVoucherResponse : BaseResponse
    {
        public Guid Id { get; set; }
    }
}
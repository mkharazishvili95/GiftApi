using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Activate
{
    public class ActivateVoucherResponse : BaseResponse
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }
}
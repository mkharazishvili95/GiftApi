using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Edit
{
    public class EditVoucherResponse : BaseResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
        public int? BrandId { get; set; }
    }
}

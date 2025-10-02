using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Manage.Commands.UpdateVoucher
{
    public class UpdateVoucherResponse : BaseResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
        public int? BrandId { get; set; }
    }
}

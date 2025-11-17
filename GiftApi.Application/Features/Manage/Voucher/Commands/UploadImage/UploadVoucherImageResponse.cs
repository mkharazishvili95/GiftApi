using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.UploadImage
{
    public class UploadVoucherImageResponse : BaseResponse
    {
        public Guid VoucherId { get; set; }
        public string? ImageUrl { get; set; }
        public int? FileId { get; set; }
    }
}
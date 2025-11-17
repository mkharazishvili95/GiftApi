using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.UploadImage
{
    public class UploadVoucherImageCommand : IRequest<UploadVoucherImageResponse>
    {
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? FileContent { get; set; }
    }
}
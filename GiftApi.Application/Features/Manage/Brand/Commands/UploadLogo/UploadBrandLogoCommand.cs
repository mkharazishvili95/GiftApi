using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.UploadLogo
{
    public class UploadBrandLogoCommand : IRequest<UploadBrandLogoResponse>
    {
        public int BrandId { get; set; }
        public string? FileName { get; set; }
        public string? FileContent { get; set; }
    }
}
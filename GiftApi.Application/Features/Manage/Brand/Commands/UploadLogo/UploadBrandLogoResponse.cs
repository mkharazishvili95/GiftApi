using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Brand.Commands.UploadLogo
{
    public class UploadBrandLogoResponse : BaseResponse
    {
        public int BrandId { get; set; }
        public string? LogoUrl { get; set; }
        public int? FileId { get; set; }
    }
}